using MyStaging.Helpers;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyStaging.Helpers
{
    public class DeleteBuilder<T> : QueryContext<T> where T : class, new()
    {
        public new DeleteBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            base.Where(predicate);
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
            string cmdText = $"DELETE FROM {tableName} {"WHERE " + string.Join("\nAND ", WhereList)}";
            return base.ExecuteNonQuery(cmdText);
        }
    }
}
