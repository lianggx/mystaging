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

    /*
    /// <summary>
    ///  创建动态类型
    /// </summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    public Type CreateDynamicType(Type entityType)
    {
        var asmName = new AssemblyName("MyDynamicAssembly_" + Guid.NewGuid());
        var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
        var moduleBuilder = asmBuilder.DefineDynamicModule("MyDynamicModule_" + Guid.NewGuid());

        TypeBuilder typeBuilder = moduleBuilder.DefineType(entityType.GetType() + "$MyDynamicType", TypeAttributes.Public);

        ConstructorBuilder ctor1 = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);

        ILGenerator ctor1IL = ctor1.GetILGenerator();
        ctor1IL.Emit(OpCodes.Ldarg_0);
        ctor1IL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
        ctor1IL.Emit(OpCodes.Ret);

        foreach (var pi in entityType.GetProperties())
        {
            PropertyBuilder propBuilder = typeBuilder.DefineProperty(pi.Name, PropertyAttributes.HasDefault, pi.PropertyType, null);
            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            //构造Get访问器
            MethodBuilder mbNumberGetAccessor = typeBuilder.DefineMethod("get_" + pi.Name, getSetAttr, pi.PropertyType, Type.EmptyTypes);
            FieldBuilder fbNumber = typeBuilder.DefineField("_" + pi.Name, pi.PropertyType, FieldAttributes.Private);

            ILGenerator numberGetIL = mbNumberGetAccessor.GetILGenerator();
            numberGetIL.Emit(OpCodes.Ldarg_0);
            numberGetIL.Emit(OpCodes.Ldfld, fbNumber);
            numberGetIL.Emit(OpCodes.Ret);

            //构造Set访问器
            MethodBuilder mbNumberSetAccessor = typeBuilder.DefineMethod("set_" + pi.Name, getSetAttr, null, new Type[] { pi.PropertyType });

            ILGenerator numberSetIL = mbNumberSetAccessor.GetILGenerator();
            // Load the instance and then the numeric argument, then store the
            // argument in the field.
            numberSetIL.Emit(OpCodes.Ldarg_0);
            numberSetIL.Emit(OpCodes.Ldarg_1);
            numberSetIL.Emit(OpCodes.Stfld, fbNumber);
            numberSetIL.Emit(OpCodes.Ret);

            propBuilder.SetGetMethod(mbNumberGetAccessor);
            propBuilder.SetSetMethod(mbNumberSetAccessor);
        }
        Type ti = typeBuilder.CreateTypeInfo().AsType();

        return ti;
    }
    */

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
