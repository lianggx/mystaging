using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;
using System.Collections.Concurrent;

namespace MyStaging.Mapping
{
    public class DynamicBuilder<T>
    {
        private static IDictionary<Type, Type> types = new Dictionary<Type, Type>();
        private static readonly MethodInfo getValueMethod = typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(int) });
        private static readonly MethodInfo isDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
        private delegate T Load(IDataRecord dataRecord);
        private Load handler;

        private DynamicBuilder() { }

        static DynamicBuilder()
        {
            types.Add(typeof(bool), typeof(Nullable<bool>));
            types.Add(typeof(byte), typeof(Nullable<byte>));
            types.Add(typeof(DateTime), typeof(Nullable<DateTime>));
            types.Add(typeof(decimal), typeof(Nullable<decimal>));
            types.Add(typeof(double), typeof(Nullable<double>));
            types.Add(typeof(float), typeof(Nullable<float>));
            types.Add(typeof(Guid), typeof(Nullable<Guid>));
            types.Add(typeof(Int16), typeof(Nullable<Int16>));
            types.Add(typeof(Int32), typeof(Nullable<Int32>));
            types.Add(typeof(Int64), typeof(Nullable<Int64>));
        }

        public T Build(IDataRecord dataRecord)
        {
            return handler(dataRecord);
        }

        public static DynamicBuilder<T> CreateBuilder(IDataRecord dataRecord)
        {
            DynamicBuilder<T> dynamicBuilder = new DynamicBuilder<T>();

            DynamicMethod method = new DynamicMethod("DynamicCreate", typeof(T),
                    new Type[] { typeof(IDataRecord) }, typeof(T), true);
            ILGenerator generator = method.GetILGenerator();

            LocalBuilder result = generator.DeclareLocal(typeof(T));
            generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);

            PropertyInfo[] pis = typeof(T).GetProperties();
            IDictionary<string, PropertyInfo> drNameList = new Dictionary<string, PropertyInfo>();
            for (int i = 0; i < pis.Length; i++)
            {
                PropertyInfo pi = pis[i];
                drNameList[pi.Name.ToLower()] = pi;
            }
            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                string fieldName = dataRecord.GetName(i);
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
                        generator.Emit(OpCodes.Box, memberType);
                    }
                    else if (unboxType == typeof(char))
                    {
                        generator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToChar", new Type[] { typeof(object) }));
                    }
                    else
                    {
                        generator.Emit(OpCodes.Unbox_Any, dataRecord.GetFieldType(i));
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

            dynamicBuilder.handler = (Load)method.CreateDelegate(typeof(Load));
            return dynamicBuilder;
        }
    }
}
