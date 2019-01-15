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

            if (this.WhereList.Count == 0)
                throw new ArgumentException("The delete operation must specify where conditions!");

            this.ToString();

            return base.ExecuteNonQuery(this.CommandText);
        }

        /// <summary>
        ///  重写方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string tableName = MyStagingUtils.GetMapping(typeof(T));
            this.CommandText = $"DELETE FROM {tableName} a {"WHERE " + string.Join("\nAND ", WhereList)}";

            return this.CommandText;
        }
    }
}
