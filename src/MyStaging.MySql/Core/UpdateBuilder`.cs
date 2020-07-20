using MySql.Data.MySqlClient;
using MyStaging.Common;
using MyStaging.Core;
using MyStaging.Interface.Core;
using MyStaging.Metadata;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace MyStaging.MySql.Core
{
    /// <summary>
    ///  数据库更新对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpdateBuilder<T> : ExpressionCondition<T>, IUpdateBuilder<T> where T : class
    {
        //  public Action<T> OnChanged = null;
        private readonly List<string> setList = new List<string>();

        public UpdateBuilder() { }

        private readonly DbContext dbContext;
        public UpdateBuilder(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        ///  该方法没有对sql注入进行参数化过滤
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public new IUpdateBuilder<T> Where(string expression)
        {
            base.Where(expression);
            return this;
        }

        public new IUpdateBuilder<T> Where(string formatCommad, params object[] pValue)
        {
            base.Where(formatCommad, pValue);
            return this;
        }

        public new IUpdateBuilder<T> Where(Expression<Func<T, bool>> predicate) => this.Where<T>(null, predicate);

        public new IUpdateBuilder<T> Where<TResult>(Expression<Func<TResult, bool>> predicate) => this.Where<TResult>(null, predicate);

        public new IUpdateBuilder<T> Where<TResult>(string alisName, Expression<Func<TResult, bool>> predicate)
        {
            base.Where(alisName, predicate);
            return this;
        }

        public override void AddParameter(string field, object value)
        {
            MySqlParameter p = new MySqlParameter(field, value);
            this.AddParameter(p);
        }

        /// <summary>
        ///  直接对当前字段执行增减操作
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IUpdateBuilder<T> SetIncrement(string field, decimal value)
        {
            setList.Add($"`{field}`=COALESCE({field},0) + {value}");
            return this;
        }

        public IUpdateBuilder<T> SetValue<TResult>(Expression<Func<T, TResult>> selector, object value)
        {
            return SetValue(MyStagingUtils.GetMemberName<T, TResult>(selector), value);
        }

        public IUpdateBuilder<T> SetValue(string field, object value)
        {
            this.AddParameter(field, value);
            setList.Add($"`{field}` = @{field}");
            return this;
        }

        /// <summary>
        ///  将当前更改保存到数据库
        /// </summary>
        /// <returns></returns>
        public T SaveChange()
        {
            DeExpression();

            CheckNotNull.NotEmpty(setList, "Fields to be updated must be provided!");
            CheckNotNull.NotEmpty(WhereConditions, "The update operation must specify where conditions!");

            this.ToSQL();
            var properties = MyStagingUtils.GetDbFields(typeof(T));
            using var reader = dbContext.ByMaster().Execute.ExecuteDataReader(CommandType.Text, CommandText, this.Parameters.ToArray());
            try
            {
                reader.Read();
                T obj = (T)Activator.CreateInstance(typeof(T));
                foreach (var pi in properties)
                {
                    var value = reader[pi.Name];
                    if (value != DBNull.Value)
                        pi.SetValue(obj, value);
                }
                return obj;
            }
            finally
            {
                Clear();
            }
        }

        /// <summary>
        ///  重写方法
        /// </summary>
        /// <returns></returns>
        public override string ToSQL()
        {
            string tableName = MyStagingUtils.GetMapping(typeof(T), ProviderType.MySql);
            this.CommandText = $"UPDATE {tableName} a SET {string.Join(",", this.setList)} {"WHERE " + string.Join("\nAND ", WhereConditions)};";
            this.CommandText += $"\n SELECT * FROM {tableName} {"WHERE " + string.Join("\nAND ", WhereConditions)};";
            return this.CommandText;
        }

        public IUpdateBuilder<T> SetArrayAppend(string field, object value)
        {
            throw new NotImplementedException();
        }

        public IUpdateBuilder<T> SetArrayRemove(string field, object value)
        {
            throw new NotImplementedException();
        }
    }
}
