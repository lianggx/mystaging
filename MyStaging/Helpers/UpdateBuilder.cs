using MyStaging.Helpers;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  数据库更新对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpdateBuilder<T> : QueryContext<T> where T : class, new()
    {
        public Action<T> OnChanged = null;
        private List<string> setList = new List<string>();

        public UpdateBuilder() { }

        public UpdateBuilder(Action<T> onChanged)
        {
            this.OnChanged = onChanged;
        }

        /// <summary>
        ///  设置字段值
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <param name="dbType">数据库数据类型</param>
        /// <param name="value">字段指定的值</param>
        /// <param name="size">字段大小</param>
        /// <param name="specificType">指定的数据库类型，通常在字段类型为枚举类型时需要指定</param>
        /// <returns></returns>
        protected UpdateBuilder<T> SetField(string field, NpgsqlDbType dbType, object value, int size, Type specificType = null)
        {
            base.AddParameter(field, dbType, value, size, specificType);
            setList.Add($"{field}=@{field}");
            return this;
        }

        /// <summary>
        ///  直接对当前字段执行增减操作
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UpdateBuilder<T> SetIncrement(string field, decimal value)
        {
            setList.Add($"{field}=COALESCE({field},0) + {value}");
            return this;
        }

        /// <summary>
        ///  对数据库数组字段进行追加值操作
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <param name="dbType">数据库数据类型</param>
        /// <param name="value">字段指定的值</param>
        /// <param name="size">字段大小</param>
        /// <param name="specificType">通常在字段类型为枚举类型时需要指定</param>
        /// <returns></returns>
        protected UpdateBuilder<T> SetArrayAppend(string field, NpgsqlDbType dbType, object value, int size, Type specificType = null)
        {
            base.AddParameter(field, dbType, value, size, specificType);
            setList.Add($"{field}=array_append({field},@{field})");
            return this;
        }

        /// <summary>
        ///  从数据库数组字段中移除指定的值
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <param name="dbType">数据库数据类型</param>
        /// <param name="value">字段指定的值</param>
        /// <param name="size">字段大小</param>
        /// <param name="specificType">通常在字段类型为枚举类型时需要指定</para
        /// <returns></returns>
        protected UpdateBuilder<T> SetArrayRemove(string field, NpgsqlDbType dbType, object value, int size, Type specificType = null)
        {
            base.AddParameter(field, dbType, value, size, specificType);
            setList.Add($"{field} = array_remove({field},@{field})");
            return this;
        }

        /// <summary>
        ///  将当前更改保存到数据库
        /// </summary>
        /// <returns></returns>
        public int SaveChange()
        {
            string tableName = MyStagingUtils.GetMapping(typeof(T));
            if (WhereExpressionList.Count > 0)
            {
                foreach (var item in WhereExpressionList)
                {
                    DbExpressionVisitor expression = new DbExpressionVisitor();
                    expression.Visit(item.Body);
                    WhereList.Add(expression.SqlText.Builder.ToString().ToLower());
                    ParamList.AddRange(expression.SqlText.Parameters);
                }
            }
            string cmdText = $"UPDATE {tableName} SET {string.Join(",", this.setList)} {"WHERE " + string.Join("\nAND ", WhereList)}";
            int affrows = 0;
            if (OnChanged != null)
            {
                cmdText += " RETURNING *;";

                var objList = base.ByMaster().ExecuteReader<T>(cmdText);
                affrows = objList.Count;
                if (affrows > 0 && this.OnChanged != null)
                {
                    OnChanged(objList[0]);
                }
            }
            else
                affrows = base.ExecuteNonQuery(cmdText);

            return affrows;
        }

    }
}
