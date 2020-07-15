using MySql.Data.MySqlClient;
using MyStaging.Common;
using MyStaging.Core;
using MyStaging.Interface.Core;
using System;
using System.Data.Common;
using System.Linq.Expressions;

namespace MyStaging.MySql.Core
{
    public class DeleteBuilder<T> : ExpressionCondition<T>, IDeleteBuilder<T> where T : class
    {
        private readonly DbContext dbContext;
        public DeleteBuilder(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        ///  该方法没有对sql注入进行参数化过滤
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public new IDeleteBuilder<T> Where(string expression)
        {
            base.Where(expression);
            return this;
        }

        public new IDeleteBuilder<T> Where(string formatCommad, params object[] pValue)
        {
            base.Where(formatCommad, pValue);
            return this;
        }

        public new IDeleteBuilder<T> Where(Expression<Func<T, bool>> predicate) => this.Where<T>(null, predicate);

        public new IDeleteBuilder<T> Where<TResult>(Expression<Func<TResult, bool>> predicate) => this.Where<TResult>(null, predicate);

        public new IDeleteBuilder<T> Where<TResult>(string alisName, Expression<Func<TResult, bool>> predicate)
        {
            base.Where(predicate);
            return this;
        }

        public override void AddParameter(string field, object value)
        {
            this.AddParameter(new MySqlParameter(field, value));
        }

        public new IDeleteBuilder<T> AddParameter(params DbParameter[] parameters)
        {
            base.AddParameter(parameters);
            return this;
        }

        /// <summary>
        ///  将当前更改保存到数据库
        /// </summary>
        /// <returns></returns>
        public int SaveChange()
        {
            DeExpression();
            CheckNotNull.NotEmpty(WhereConditions, "The delete operation must specify where conditions!");
            this.ToSQL();
            var affrows = dbContext.Execute.ExecuteNonQuery(System.Data.CommandType.Text, this.CommandText, Parameters.ToArray());

            return affrows;
        }

        /// <summary>
        ///  重写方法
        /// </summary>
        /// <returns></returns>
        public override string ToSQL()
        {
            string tableName = MyStagingUtils.GetMapping(typeof(T), ProviderType.MySql);
            this.CommandText = $"DELETE FROM {tableName} {"WHERE " + string.Join("\nAND ", WhereConditions)};";

            return this.CommandText;
        }
    }
}
