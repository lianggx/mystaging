using MyStaging.Helpers;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace MyStaging.Helpers
{
    public class UpdateBuilder<T> : QueryContext<T> where T : class, new()
    {
        private List<string> setList = new List<string>();
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
        public UpdateBuilder<T> SetIncrement(string field, int value)
        {
            setList.Add($"{field}={field} + {value}");
            return this;
        }

        protected UpdateBuilder<T> SetArrayAppend(string field, NpgsqlDbType dbType, object value, int size, Type specificType = null)
        {
            base.AddParameter(field, dbType, value, size, specificType);
            setList.Add($"{field}=array_append({field},@{field})");
            return this;
        }

        protected UpdateBuilder<T> SetArrayRemove(string field, NpgsqlDbType dbType, object value, int size, Type specificType = null)
        {
            base.AddParameter(field, dbType, value, size, specificType);
            setList.Add($"{field} = array_remove({field},@{field})");
            return this;
        }

        public int SaveChange()
        {
            string tableName = MyStagingUtils.GetMapping(typeof(T));
            if (WhereExpressionList.Count > 0)
            {
                foreach (var item in WhereExpressionList)
                {
                    PgSqlExpression expression = new PgSqlExpression();
                    expression.ExpressionCapture(item.Body);
                    WhereList.Add(expression.CommandText.ToString().ToLower());
                    ParamList.AddRange(expression.Parameters);
                }
            }
            string cmdText = $"UPDATE {tableName} SET {string.Join(",", this.setList)} {"WHERE " + string.Join("\nAND ", WhereList)}";
            return base.ExecuteNonQuery(cmdText);
        }

    }
}
