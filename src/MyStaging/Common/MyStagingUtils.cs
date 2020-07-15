using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace MyStaging.Common
{
    public class MyStagingUtils
    {
        public static List<PropertyInfo> GetDbFields(Type type)
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
                    tableName = $"`{mapping.Schema}`.`{tableName}`";
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
