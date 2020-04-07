using MyStaging.Mapping;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  数据库实体查找对象
    /// </summary>
    public class MyStagingUtils
    {
        /// <summary>
        ///  根据传入的实体对象获得数据库架构级表的映射名称
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetMapping(Type t)
        {
            string tableName = string.Empty;
            TypeInfo typeInfo = t.GetTypeInfo();
            EntityMappingAttribute mapping = typeInfo.GetCustomAttribute(typeof(EntityMappingAttribute)) as EntityMappingAttribute;
            if (mapping != null)
            {
                tableName = mapping.Name;
                if (!string.IsNullOrEmpty(mapping.Schema))
                {
                    tableName = mapping.Schema + "." + tableName;
                }
            }
            else
                throw new NotSupportedException("在表连接实体上找不到特性 EntityMappingAttribute ，请确认数据库实体模型");

            return tableName;
        }


        /// <summary>
        ///  复制两个对象的属性值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="targetObj">待赋值的目标对象</param>
        /// <param name="sourceObj">复制的源对象</param>
        /// <param name="flags">指定属性搜索范围</param>
        public static void CopyProperty<T>(T targetObj, T sourceObj, BindingFlags flags = BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Default | BindingFlags.Public)
        {
            PropertyInfo[] properties = sourceObj.GetType().GetProperties(flags);

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo pi = properties[i];
                if (pi.CanWrite)
                    pi.SetValue(targetObj, pi.GetValue(sourceObj, null), null);
            }
        }

        private static readonly MethodInfo getValueMethod = typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(int) });
        private static readonly MethodInfo isDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });

        public static Delegate CreateDynamicDelegate(Type targetType, IDataRecord dr, Type load)
        {
            DynamicMethod method = new DynamicMethod("DynamicCreate", targetType,
                    new Type[] { typeof(IDataRecord) }, targetType, true);
            ILGenerator generator = method.GetILGenerator();

            LocalBuilder result = generator.DeclareLocal(targetType);
            generator.Emit(OpCodes.Newobj, targetType.GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);

            PropertyInfo[] pis = targetType.GetProperties();
            IDictionary<string, PropertyInfo> drNameList = new Dictionary<string, PropertyInfo>();
            for (int i = 0; i < pis.Length; i++)
            {
                PropertyInfo pi = pis[i];
                drNameList[pi.Name.ToLower()] = pi;
            }
            for (int i = 0; i < dr.FieldCount; i++)
            {
                string fieldName = dr.GetName(i).ToLower();
                if (drNameList.ContainsKey(fieldName) == false)
                    continue;

                PropertyInfo propertyInfo = drNameList[fieldName];
                Label endIfLabel = generator.DefineLabel();

                if (propertyInfo != null && propertyInfo.GetSetMethod() != null)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                    generator.Emit(OpCodes.Brtrue, endIfLabel);

                    generator.Emit(OpCodes.Ldloc, result);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, getValueMethod);

                    Type memberType = propertyInfo.PropertyType;
                    Type nullUnderlyingType = Nullable.GetUnderlyingType(memberType);
                    Type unboxType = nullUnderlyingType != null ? nullUnderlyingType : memberType;
                    bool isEnum = unboxType.GetTypeInfo().IsEnum;
                    if (unboxType == typeof(byte[]) || unboxType == typeof(string))
                    {
                        generator.Emit(OpCodes.Castclass, memberType);
                    }
                    else if (isEnum)
                    {
                        generator.Emit(OpCodes.Unbox_Any, memberType);
                    }
                    else if (unboxType == typeof(char))
                    {
                        generator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToChar", new Type[] { typeof(object) }));
                    }
                    else if (unboxType == typeof(JToken))
                    {
                        generator.Emit(OpCodes.Call, typeof(JToken).GetMethod("Parse", new Type[] { typeof(string) }));
                    }
                    else
                    {
                        generator.Emit(OpCodes.Unbox_Any, dr.GetFieldType(i));
                        if (nullUnderlyingType != null)
                        {
                            generator.Emit(OpCodes.Newobj, memberType.GetConstructor(new[] { nullUnderlyingType }));
                        }
                    }

                    generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
                    generator.MarkLabel(endIfLabel);
                }
            }

            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);
            return method.CreateDelegate(load);
        }
    }
}
