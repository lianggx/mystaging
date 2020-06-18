using MyStaging.Common;
using MyStaging.Interface;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace MyStaging.Core
{
    public abstract class ExpressionCondition<T>
    {
        /// <summary>
        ///  该方法没有对sql注入进行参数化过滤
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual void Where(string expression)
        {
            if (string.IsNullOrEmpty(expression)) throw new ArgumentNullException("必须传递参数 expression");

            WhereConditions.Add($"({expression})");
        }

        /// <summary>
        ///  增加查询条件
        /// </summary>
        /// <param name="formatCommad">格式为{0}{1}的字符串</param>
        /// <param name="pValue">{0}{1}对应的值</param>
        /// <returns></returns>
        public virtual void Where(string formatCommad, params object[] pValue)
        {
            if (pValue == null || pValue.Length == 0) throw new ArgumentNullException("必须传递参数 pValue");
            List<object> nameList = new List<object>();
            foreach (var item in pValue)
            {
                string name = Guid.NewGuid().ToString("N");
                this.AddParameter(name, item);
                nameList.Add("@" + name);
            }
            var expression = string.Format(formatCommad, nameList.ToArray());
            this.Where(expression);
        }

        /// <summary>
        ///  增加查询条件
        /// </summary>
        /// <param name="predicate">查询表达式</param>
        /// <returns></returns>
        public virtual void Where(Expression<Func<T, bool>> predicate) => Where<T>(null, predicate);

        /// <summary>
        ///  增加查询条件
        /// </summary>
        /// <typeparam name="TResult">查询表达式的对象</typeparam>
        /// <param name="predicate">查询表达式</param>
        /// <returns></returns>
        public virtual void Where<TResult>(Expression<Func<TResult, bool>> predicate) => this.Where<TResult>(null, predicate);

        /// <summary>
        ///  增加查询条件
        /// </summary>
        /// <typeparam name="TResult">查询表达式的对象</typeparam>
        /// <param name="alisName">alisName</param>
        /// <param name="predicate">查询表达式</param>
        /// <returns></returns>
        public virtual void Where<TResult>(string alisName, Expression<Func<TResult, bool>> predicate)
        {
            ExpressionModel em = new ExpressionModel
            {
                Body = predicate.Body,
                Model = typeof(TResult),
                UnionAlisName = alisName
            };
            WhereExpressions.Add(em);
        }

        /// <summary>
        ///  增加一个查询参数
        /// </summary>
        /// <param name="field">数据库字段</param>
        /// <param name="value">字段指定的值</param>
        /// <returns></returns>
        public abstract void AddParameter(string field, object value);

        /// <summary>
        /// 增加一组查询参数
        /// </summary>
        /// <param name="parameters">输入参数</param>
        /// <returns></returns>
        public virtual void AddParameter(params DbParameter[] parameters)
        {
            CheckNotNull.NotEmpty(parameters, nameof(parameters));
            Parameters.AddRange(parameters);
        }

        public void DeExpression()
        {
            if (WhereExpressions.Count > 0)
            {
                foreach (var item in WhereExpressions)
                {
                    DbExpressionVisitor expression = new DbExpressionVisitor();
                    expression.Visit(item.Body);
                    WhereConditions.Add(expression.SqlText.Builder.ToString().ToLower());
                    foreach (var p in expression.SqlText.Parameters)
                    {
                        AddParameter(p.Name, p.Value);
                    }
                }
            }
        }

        public abstract string ToSQL();

        /// <summary>
        ///  清除参数列表
        /// </summary>
        public virtual void Clear()
        {
            this.Parameters.Clear();
            this.WhereConditions.Clear();
            this.WhereExpressions.Clear();
            this.CommandText = null;
        }

        /// <summary>
        ///  获取或者设置参数列表
        /// </summary>
        public List<DbParameter> Parameters { get; set; } = new List<DbParameter>();

        /// <summary>
        ///  获取或者设置查询表达式列表
        /// </summary>
        public List<ExpressionModel> WhereExpressions { get; } = new List<ExpressionModel>();

        /// <summary>
        ///  获取或者设置查询条件列表
        /// </summary>
        public List<string> WhereConditions { get; set; } = new List<string>();

        public string CommandText { get; set; }
    }
}
