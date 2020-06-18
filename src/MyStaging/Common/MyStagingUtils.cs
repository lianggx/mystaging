using MyStaging.Mapping;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace MyStaging.Common
{
    public class MyStagingUtils
    {
        public static List<PropertyInfo> GetProperties(Type type)
        {
            var properties = new List<PropertyInfo>();
            var pis = type.GetProperties();
            for (int j = 0; j < pis.Length; j++)
            {
                PropertyInfo pi = pis[j];
                var attr = pi.GetCustomAttribute(typeof(NotMappedAttribute));
                if (attr != null) continue;
                properties.Add(pi);
            }

            return properties;
        }

        /// <summary>
        ///  根据传入的实体对象获得数据库架构级表的映射名称
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetMapping(Type t)
        {
            TypeInfo typeInfo = t.GetTypeInfo();
            string tableName;
            if (typeInfo.GetCustomAttribute(typeof(TableAttribute)) is TableAttribute mapping)
            {
                tableName = mapping.Name;
                if (!string.IsNullOrEmpty(mapping.Schema))
                {
                    tableName = mapping.Schema + "." + tableName;
                }
            }
            else
                throw new NotSupportedException("在表连接实体上找不到特性 TableAttribute ，请确认数据库实体模型");

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
            DynamicMethod method = new DynamicMethod("DynamicCreate", targetType, new Type[] { typeof(IDataRecord) }, targetType, true);
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
                    Type unboxType = nullUnderlyingType ?? memberType;
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

        public static T ConvertEnum<V, T>(V obj)
        {
            if (obj == null)
                return default;

            var targetType = typeof(T);
            if (targetType.AssemblyQualifiedName.StartsWith("System.Nullable`1"))
            {
                targetType = targetType.GenericTypeArguments[0];
            }

            Object value = Enum.Parse(targetType, obj.ToString());
            if (value == null)
            {
                return default;
            }
            return (T)value;
        }

        protected static bool IsTuple(Type type) => type.Namespace == "System" && type.Name.StartsWith("ValueTuple`");

        public static TResult ReadObj<TResult>(ref DynamicBuilder<TResult> builder, DbDataReader dr, Type objType)
        {
            bool isTuple = IsTuple(objType);
            bool isEnum = objType.IsEnum;
            TResult obj;
            if (isTuple)
            {
                int columnIndex = -1;
                obj = (TResult)GetValueTuple(objType, dr, ref columnIndex);
            }
            else if (IsValueType(objType) || isEnum)
            {
                obj = (TResult)GetValueType(objType, dr);
            }
            else if (objType.Namespace != null && objType.Namespace.StartsWith("Newtonsoft"))
            {
                obj = (TResult)GetJToken(dr);
            }
            else
            {
                if (builder == null)
                {
                    builder = DynamicBuilder<TResult>.CreateBuilder(dr);
                }
                obj = builder.Build(dr);
            }
            return obj;
        }

        public static object ReadObj(DynamicBuilder builder, DbDataReader dr, Type objType)
        {
            bool isTuple = IsTuple(objType);
            bool isEnum = objType.IsEnum;
            object obj;
            if (isTuple)
            {
                int columnIndex = -1;
                obj = GetValueTuple(objType, dr, ref columnIndex);
            }
            else if (IsValueType(objType) || isEnum)
            {
                obj = GetValueType(objType, dr);
            }
            else if (objType.Namespace != null && objType.Namespace.StartsWith("Newtonsoft"))
            {
                obj = GetJToken(dr);
            }
            else
            {
                obj = builder.Build(dr);
            }

            return obj;
        }

        /// <summary>
        ///  检查查询结果对象是否为元组类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected static bool IsValueType(Type type)
        {
            return (type.Namespace == "System" && type.Name.StartsWith("String")) || (type.BaseType == typeof(ValueType));
        }

        /// <summary>
        ///  将查询结果转换为元组对象
        /// </summary>
        /// <param name="objType">元组类型</param>
        /// <param name="dr">查询流</param>
        /// <param name="columnIndex">dr index</param>
        /// <returns></returns>
        protected static object GetValueTuple(Type objType, IDataReader dr, ref int columnIndex)
        {
            bool isTuple = objType.Namespace == "System" && objType.Name.StartsWith("ValueTuple`");
            if (isTuple)
            {
                FieldInfo[] fs = objType.GetFields();
                Type[] types = new Type[fs.Length];
                object[] parameters = new object[fs.Length];
                for (int i = 0; i < fs.Length; i++)
                {
                    types[i] = fs[i].FieldType;
                    parameters[i] = GetValueTuple(types[i], dr, ref columnIndex);
                }
                ConstructorInfo info = objType.GetConstructor(types);
                return info.Invoke(parameters);
            }
            ++columnIndex;
            object dbValue = dr[columnIndex];
            dbValue = dbValue is DBNull ? null : dbValue;

            return dbValue;
        }

        /// <summary>
        ///  从数据库流中读取值并转换为指定的对象类型
        /// </summary>
        /// <param name="objType">对象类型</param>
        /// <param name="dr">查询流</param>
        /// <returns></returns>
        protected static object GetValueType(Type objType, IDataReader dr)
        {
            object dbValue = dr[0];
            dbValue = dbValue is DBNull ? null : dbValue;
            dbValue = Convert.ChangeType(dbValue, objType);

            return dbValue;
        }

        /// <summary>
        ///  将查询结果转换为 JToken 对象
        /// </summary>
        /// <param name="dr">查询流</param>
        /// <returns></returns>
        protected static object GetJToken(IDataReader dr)
        {
            object dbValue = dr[0];
            if (dbValue is DBNull)
                return null;
            else
                return JToken.Parse(dbValue.ToString());
        }

        /// <summary>
        ///  获取表达式成员名称
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static string GetMemberName<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            MemberExpression exp;
            if (selector.Body.NodeType == ExpressionType.Convert)
            {
                exp = (MemberExpression)((UnaryExpression)selector.Body).Operand;
            }
            else
                exp = (MemberExpression)selector.Body;

            return exp.Member.Name;
        }
    }
}
