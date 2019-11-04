using MyStaging.Common;
using MyStaging.Helpers;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyStaging.Helpers
{
    public class DeleteBuilder<T> : QueryContext<T> where T : class, new()
    {
        /// <summary>
        ///  增加删除的操作条件
        /// </summary>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public new DeleteBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            base.Where(predicate);
            return this;
        }

        /// <summary>
        ///  将当前更改保存到数据库
        /// </summary>
        /// <returns></returns>
        public int SaveChange()
        {
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

            CheckNotNull.NotEmpty(this.WhereList, "The delete operation must specify where conditions!");

            this.ToSQL();
            var affrows = 0;
            try
            {
                affrows = base.ExecuteNonQuery(this.CommandText);
            }
            finally
            {
                base.ParamList.Clear();
            }

            return affrows;
        }

        /// <summary>
        ///  重写方法
        /// </summary>
        /// <returns></returns>
        public new string ToSQL()
        {
            string tableName = MyStagingUtils.GetMapping(typeof(T));
            this.CommandText = $"DELETE FROM {tableName} a {"WHERE " + string.Join("\nAND ", WhereList)}";

            return this.CommandText;
        }
    }
}
