//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Data;
//using System.Collections.Concurrent;
//using Newtonsoft.Json.Linq;

//namespace MyStaging.Mapping
//{
//    /// <summary>
//    ///  动态代码执行对象
//    /// </summary>
//    public class PropertyCopy<T>
//    {
//        private static IDictionary<Type, Type> types = new Dictionary<Type, Type>();
//        private static readonly MethodInfo setValueMethod = typeof(IDataRecord).GetMethod("set_value", new Type[] { typeof(int) });
//        private Action<T, T> Load = null;

//        /// <summary>
//        ///  默认构造函数
//        /// </summary>
//        private PropertyCopy() { }

//        /// <summary>
//        ///  静态构造函数，初始化各种可空值类型的的映射
//        /// </summary>
//        static PropertyCopy()
//        {
//            types.Add(typeof(bool), typeof(Nullable<bool>));
//            types.Add(typeof(byte), typeof(Nullable<byte>));
//            types.Add(typeof(DateTime), typeof(Nullable<DateTime>));
//            types.Add(typeof(decimal), typeof(Nullable<decimal>));
//            types.Add(typeof(double), typeof(Nullable<double>));
//            types.Add(typeof(float), typeof(Nullable<float>));
//            types.Add(typeof(Guid), typeof(Nullable<Guid>));
//            types.Add(typeof(Int16), typeof(Nullable<Int16>));
//            types.Add(typeof(Int32), typeof(Nullable<Int32>));
//            types.Add(typeof(Int64), typeof(Nullable<Int64>));
//        }

//        /// <summary>
//        ///  根据传入的数据库记录读取并封装到 T 对象中
//        /// </summary>
//        /// <returns></returns>
//        public void Build(T oldModel, T newModel)
//        {
//            Load(oldModel, newModel);
//        }

//        /// <summary>
//        ///  创建动态执行代码
//        /// </summary>
//        /// <param name="dataRecord"></param>
//        /// <returns></returns>
//        public static PropertyCopy<T> CreateBuilder(T oldModel, T newModel)
//        {
//            PropertyCopy<T> dynamicBuilder = new PropertyCopy<T>();

//            DynamicMethod method = new DynamicMethod("DynamicCreate", typeof(T),
//                    new Type[] { typeof(IDataRecord) }, typeof(T), true);
//            ILGenerator generator = method.GetILGenerator();

//            LocalBuilder result = generator.DeclareLocal(typeof(T));
//            generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
//            generator.Emit(OpCodes.Stloc, result);

//            PropertyInfo[] pis = typeof(T).GetProperties();
//            for (int i = 0; i < pis.Length; i++)
//            {
//                PropertyInfo propertyInfo = pis[i];
//                Label endIfLabel = generator.DefineLabel();

//                if (propertyInfo.CanWrite)
//                {
//                    generator.Emit(OpCodes.Ldarg_0);
//                    generator.Emit(OpCodes.Ldc_I4, i);
//                    generator.Emit(OpCodes.Brtrue, endIfLabel);

//                    generator.Emit(OpCodes.Ldloc, result);
//                    generator.Emit(OpCodes.Ldarg_0);
//                    generator.Emit(OpCodes.Ldc_I4, i);
//                    generator.Emit(OpCodes.Callvirt, setValueMethod);
//                    generator.Emit(OpCodes.Call,);


//                    generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());

//                    generator.MarkLabel(endIfLabel);
//                }
//            }

//            generator.Emit(OpCodes.Ldloc, result);
//            generator.Emit(OpCodes.Ret);

//            dynamicBuilder.handler = (Load)method.CreateDelegate(typeof(Load));
//            return dynamicBuilder;
//        }
//    }
//}
