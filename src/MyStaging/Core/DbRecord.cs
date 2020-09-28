using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MyStaging.Core
{
    public abstract class DbRecord
    {
        public TResult GetResult<TResult>(DbDataReader dr)
        {
            Type resultType = typeof(TResult);
            bool isEnum = resultType.IsEnum;

            TResult result;
            if (resultType == typeof(JsonElement))
            {
                result = (TResult)GetJsonElement(dr);
            }
            else if (IsValueType(resultType))
            {
                int columnIndex = -1;
                result = (TResult)GetValueTuple(resultType, dr, ref columnIndex);
            }
            else if (isEnum)
            {
                result = (TResult)GetValueType(resultType, dr);
            }
            else
            {
                result = Activator.CreateInstance<TResult>();
                var properties = resultType.GetProperties();
                foreach (var pi in properties)
                {
                    var value = dr[pi.Name];
                    if (value == DBNull.Value)
                        continue;
                    else if (pi.PropertyType.Name == "JsonElement")
                        pi.SetValue(result, JsonDocument.Parse(value.ToString()).RootElement);
                    else
                        pi.SetValue(result, value);
                }
            }


            return result;
        }

        /// <summary>
        ///  检查查询结果对象是否为元组类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValueType(Type type)
        {
            return (type.Namespace == "System" && type.Name.StartsWith("String")) || (type.BaseType == typeof(ValueType));
        }

        /// <summary>
        ///  从数据库流中读取值并转换为指定的对象类型
        /// </summary>
        /// <param name="objType">对象类型</param>
        /// <param name="dr">查询流</param>
        /// <returns></returns>
        public object GetValueType(Type objType, IDataReader dr)
        {
            object dbValue = dr[0];
            dbValue = dbValue is DBNull ? null : dbValue;
            dbValue = Convert.ChangeType(dbValue, objType);

            return dbValue;
        }

        /// <summary>
        ///  将查询结果转换为元组对象
        /// </summary>
        /// <param name="objType">元组类型</param>
        /// <param name="dr">查询流</param>
        /// <param name="columnIndex">dr index</param>
        /// <returns></returns>
        public object GetValueTuple(Type objType, IDataReader dr, ref int columnIndex)
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
        ///  将查询结果转换为 JsonElement 对象
        /// </summary>
        /// <param name="dr">查询流</param>
        /// <returns></returns>
        public object GetJsonElement(IDataReader dr)
        {
            object dbValue = dr[0];
            if (dbValue is DBNull)
                return null;
            else
                return JsonDocument.Parse(dbValue.ToString()).RootElement;
        }
    }
}
