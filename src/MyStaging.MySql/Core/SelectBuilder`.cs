using MySql.Data.MySqlClient;
using MyStaging.Common;
using MyStaging.Core;
using MyStaging.Interface;
using MyStaging.Interface.Core;
using MyStaging.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MyStaging.MySql.Core
{
    /// <summary>
    ///  数据库查询上下文对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelectBuilder<T> : ExpressionCondition<T>, ISelectBuilder<T> where T : class
    {
        private readonly DbContext dbContext;
        private const string masterAlisName = "a";

        public SelectBuilder(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        ///  设置当前查询的 limit 和 offset
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public ISelectBuilder<T> Page(int page, int size)
        {
            page = page <= 1 ? 0 : page - 1;
            string limit = page == 0 ? "" : $"\nOFFSET {page * size}";
            LimitText = $"LIMIT {size}{limit}";
            return this;
        }

        /// <summary>
        ///  设置当前查询分组条件
        /// </summary>
        /// <param name="groupByText"></param>
        /// <returns></returns>
        public ISelectBuilder<T> GroupBy(string groupByText)
        {
            GroupByText = $"GROUP BY {groupByText}";
            return this;
        }

        /// <summary>
        ///  设置查询的排序条件，格式为：字段 ASC，如果有多个排序，以逗号分隔
        /// </summary>
        /// <param name="sortText"></param>
        /// <returns></returns>
        public ISelectBuilder<T> OrderBy(string sortText)
        {
            OrderByText = $"ORDER BY {sortText}";
            return this;
        }

        /// <summary>
        ///  设置查询的排序条件，该方法将根据表达式自动推导出排序的字段，排序方向为 ASC
        /// </summary>
        /// <typeparam name="TKey">字段</typeparam>
        /// <param name="keySelector">字段选择器</param>
        /// <returns></returns>
        public ISelectBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector) => OrderTransafer(keySelector);

        /// <summary>
        ///  设置查询的排序条件，该方法将根据表达式自动推导出排序的字段，排序方向为 ASC
        /// </summary>
        /// <typeparam name="TSource">排序字段对象</typeparam>
        /// <typeparam name="TKey">字段</typeparam>
        /// <param name="keySelector">字段选择器</param>
        /// <returns></returns>
        public ISelectBuilder<T> OrderBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector) => OrderTransafer(keySelector);

        /// <summary>
        ///  设置查询的排序条件，该方法将根据表达式自动推导出排序的字段，排序方向为 DESC
        /// </summary>
        /// <typeparam name="TKey">字段</typeparam>
        /// <param name="keySelector">字段选择器</param>
        /// <returns></returns>
        public ISelectBuilder<T> OrderByDescing<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return OrderTransafer(keySelector.Body, "DESC");
        }

        /// <summary>
        ///  设置查询的排序条件，该方法将根据表达式自动推导出排序的字段，排序方向为 DESC
        /// </summary>
        /// <typeparam name="TSource">排序字段对象</typeparam>
        /// <typeparam name="TKey">字段</typeparam>
        /// <param name="keySelector">字段选择器</param>
        /// <returns></returns>
        public ISelectBuilder<T> OrderByDescing<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector) => OrderTransafer(keySelector.Body, "DESC");

        /// <summary>
        ///  设置查询排序条件和方向
        /// </summary>
        /// <param name="keySelector">字段选择器</param>
        /// <param name="direction">排序方向，默认 ASC</param>
        /// <returns></returns>
        private ISelectBuilder<T> OrderTransafer(Expression keySelector, string direction = "ASC")
        {
            MemberExpression exp = null;
            if (keySelector.NodeType == ExpressionType.Lambda)
                exp = (MemberExpression)((LambdaExpression)keySelector).Body;
            else
                exp = (MemberExpression)keySelector;
            string alisname = UnionList.FirstOrDefault(f => f.Model.Equals(exp.Member.DeclaringType))?.AlisName;
            if (!string.IsNullOrEmpty(alisname))
            {
                alisname += ".";
            }
            else if (UnionList.Count > 0)
            {
                alisname = masterAlisName + ".";
            }
            OrderByText = $"ORDER BY {alisname}{exp.Member.Name} {direction}";
            return this;
        }

        /// <summary>
        ///  设置查询的 having 条件，如果使用该方法，必须设置 group by 条件
        /// </summary>
        /// <param name="havingText"></param>
        /// <returns></returns>
        public ISelectBuilder<T> Having(string havingText)
        {
            HavingText = $"HAVING {havingText}";
            return this;
        }

        /// <summary>
        ///  查询的结果总行数
        /// </summary>
        /// <returns></returns>
        public long Count() => ToScalar<long>("COALESCE(COUNT(1),0)");

        /// <summary>
        /// 查询的结果最大值
        /// </summary>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <param name="field">筛选字段名</param>
        /// <returns></returns>
        public virtual TResult Max<TResult>(string field) => ToScalar<TResult>($"COALESCE(MAX({field}),0)");

        /// <summary>
        /// 查询的结果最大值
        /// </summary>
        /// <param name="selector">字段选择器</param>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <returns></returns>
        public TResult Max<TResult>(Expression<Func<T, TResult>> selector) => Max<T, TResult>(selector);

        /// <summary>
        /// 查询的结果最大值
        /// </summary>
        /// <param name="selector">字段选择器</param>
        /// <typeparam name="TSource">查询目标对象</typeparam>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <returns></returns>
        public TResult Max<TSource, TResult>(Expression<Func<TSource, TResult>> selector) => Max<TResult>(MyStagingUtils.GetMemberName(selector));

        /// <summary>
        /// 查询的结果最小值
        /// </summary>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <param name="field">筛选字段名</param>
        /// <returns></returns>
        public TResult Min<TResult>(string field) => ToScalar<TResult>($"COALESCE(MIN({field}),0)");

        /// <summary>
        /// 查询的结果最小值
        /// </summary>
        /// <param name="selector">字段选择器</param>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <returns></returns>
        public TResult Min<TResult>(Expression<Func<T, TResult>> selector) => Min<T, TResult>(selector);

        /// <summary>
        /// 查询的结果最小值
        /// </summary>
        /// <param name="selector">字段选择器</param>
        /// <typeparam name="TSource">查询目标对象</typeparam>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <returns></returns>
        public TResult Min<TSource, TResult>(Expression<Func<TSource, TResult>> selector) => Min<TResult>(MyStagingUtils.GetMemberName(selector));

        /// <summary>
        /// 对查询进行求和
        /// </summary>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <param name="field">筛选字段名</param>
        /// <returns></returns>
        public TResult Sum<TResult>(string field) => ToScalar<TResult>($"COALESCE(SUM({field}),0)");

        /// <summary>
        /// 对查询进行求和
        /// </summary>
        /// <param name="selector">字段选择器</param>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <returns></returns>
        public TResult Sum<TResult>(Expression<Func<T, TResult>> selector) => Sum<T, TResult>(selector);

        /// <summary>
        /// 对查询进行求和
        /// </summary>
        /// <param name="selector">字段选择器</param>
        /// <typeparam name="TSource">查询目标对象</typeparam>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <returns></returns>
        public TResult Sum<TSource, TResult>(Expression<Func<TSource, TResult>> selector) => Sum<TResult>(MyStagingUtils.GetMemberName(selector));

        /// <summary>
        /// 对查询进行求平均值
        /// </summary>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <param name="field">筛选字段名</param>
        /// <returns></returns>
        public TResult Avg<TResult>(string field) => ToScalar<TResult>($"COALESCE(AVG({field}),0)");

        /// <summary>
        /// 对查询进行求平均值
        /// </summary>
        /// <param name="selector">字段选择器</param>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <returns></returns>
        public TResult Avg<TResult>(Expression<Func<T, TResult>> selector) => Avg<T, TResult>(selector);

        /// <summary>
        /// 对查询进行求平均值
        /// </summary>
        /// <param name="selector">字段选择器</param>
        /// <typeparam name="TSource">查询目标对象</typeparam>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <returns></returns>
        public TResult Avg<TSource, TResult>(Expression<Func<TSource, TResult>> selector) => Avg<TResult>(MyStagingUtils.GetMemberName(selector));

        /// <summary>
        ///  查询返回第一行第一列字段的值
        /// </summary>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <param name="field">查询的字段</param>
        /// <returns></returns>
        public TResult ToScalar<TResult>(string field)
        {
            if (!string.IsNullOrEmpty(GroupByText))
            {
                throw new ArgumentException("聚合查询不允许使用 GROUP BY 条件！");
            }

            Fields.Clear();
            Fields.Add(field);
            string cmdText = ToSQL();
            SQLExecute execute = byMaster ? dbContext.ByMaster().Execute : dbContext.Execute;
            object result = execute.ExecuteScalar(CommandType.Text, cmdText, Parameters.ToArray());

            return result == null ? default : (TResult)result;
        }

        /// <summary>
        ///  查询返回一行数据
        /// </summary>
        /// <typeparam name="TResult">接受查询结果对象类型</typeparam>
        /// <param name="fields">指定查询的字段</param>
        /// <returns></returns>
        public TResult ToOne<TResult>(params string[] fields)
        {
            return ToOne<TResult>(true, fields);
        }

        /// <summary>
        ///  查询返回一行数据
        /// </summary>
        /// <param name="cacheing">是否使用缓存结果，cacheing=true时，直接查询数据库，无论cacheing是否为true，最终都会根据全局缓存设置更新缓存</param>
        /// <typeparam name="TResult">接受查询结果对象类型</typeparam>
        /// <param name="fields">指定查询的字段</param>
        /// <returns></returns>
        public TResult ToOne<TResult>(bool cacheing, params string[] fields)
        {
            Page(1, 1);
            ResetFields(fields);

            var objType = typeof(TResult);
            var enable = Cacheing && dbContext.CacheManager != null && objType.IsClass;
            if (enable && cacheing)
            {
                // 读取缓存
                TResult obj = dbContext.CacheManager.GetItemCache<TResult, DbParameter>(this.Parameters);
                if (obj != null)
                {
                    this.Clear();
                    return obj;
                }
            }
            List<TResult> list = ExecuteReader<TResult>(CommandText);

            if (list.Count > 0)
            {
                var result = list[0];
                if (enable)
                {
                    dbContext.CacheManager.SetItemCache(result, this.Expire);
                }
                this.Clear();
                return result;
            }
            else
                return default;
        }

        /// <summary>
        ///  查询返回一行数据
        /// </summary>
        /// <param name="fields">指定查询的字段</param>
        /// <returns></returns>
        public T ToOne(params string[] fields) => this.ToOne<T>(fields);

        /// <summary>
        ///  查询返回一个结果集
        /// </summary>
        /// <param name="fields">指定查询的字段</param>
        /// <returns></returns>
        public List<T> ToList(params string[] fields) => ToList<T>(fields);

        /// <summary>
        ///  查询返回一个结果集
        /// </summary>
        /// <typeparam name="TResult">接受查询结果对象类型</typeparam>
        /// <param name="fields">指定查询的字段</param>
        /// <returns></returns>
        public List<TResult> ToList<TResult>(params string[] fields)
        {
            ResetFields(fields);
            return ExecuteReader<TResult>(CommandText);
        }

        private void ResetFields(params string[] fields)
        {
            Fields.Clear();
            if (fields == null || fields.Length == 0)
            {
                Fields.Add(string.Format("{0}.*", masterAlisName));
            }
            else
            {
                foreach (var item in fields)
                {
                    Fields.Add(item);
                }
            }
            ToSQL();
        }

        /// <summary>
        ///  执行查询并返回结果集
        /// </summary>
        /// <typeparam name="TResult">接受查询结果对象类型</typeparam>
        /// <returns></returns>
        public List<TResult> ExecuteReader<TResult>(string cmdText)
        {
            List<TResult> list = new List<TResult>();
            SQLExecute execute = byMaster ? dbContext.ByMaster().Execute : dbContext.Execute;
            using var reader = execute.ExecuteDataReader(CommandType.Text, cmdText, Parameters.ToArray());
            while (reader.Read())
            {
                var obj = GetResult<TResult>(reader);
                list.Add(obj);
            };

            this.Clear();

            return list;
        }

        /// <summary>
        ///  该方法没有对sql注入进行参数化过滤
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public new ISelectBuilder<T> Where(string expression)
        {
            base.Where(expression);
            return this;
        }

        public new ISelectBuilder<T> Where(string formatCommad, params object[] pValue)
        {
            base.Where(formatCommad, pValue);
            return this;
        }

        public new ISelectBuilder<T> Where(Expression<Func<T, bool>> predicate) => this.Where<T>(null, predicate);

        public new ISelectBuilder<T> Where<TResult>(Expression<Func<TResult, bool>> predicate) => this.Where<TResult>(null, predicate);

        public new ISelectBuilder<T> Where<TResult>(string alisName, Expression<Func<TResult, bool>> predicate)
        {
            base.Where(alisName, predicate);
            return this;
        }

        public override void AddParameter(string field, object value)
        {
            this.AddParameter(new MySqlParameter(field, value));
        }
        #region 连接查询

        /// <summary>
        ///  执行表连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="unionType">连接类型</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> Union<TModel>(string alisName, UnionType unionType, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, unionType, predicate);

        /// <summary>
        ///  执行表连接查询，该方法可连接两个不同的表
        /// </summary>
        /// <typeparam name="TModel1">要连接的数据库实体对象</typeparam>
        /// <typeparam name="TModel2">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">主表的别名</param>
        /// <param name="unionAlisName">连接表的别名</param>
        /// <param name="unionType">连接类型</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> Union<TModel1, TModel2>(string alisName, string unionAlisName, UnionType unionType, Expression<Func<TModel1, TModel2, bool>> predicate)
        {
            Type type = typeof(TModel1);
            var last = UnionList.Where(f => f.Model.Equals(type) && f.UnionAlisName == alisName).FirstOrDefault();

            if (alisName != masterAlisName && last == null)
            {
                ExpressionUnionInfo u2 = new ExpressionUnionInfo
                {
                    Model = typeof(TModel1),
                    MasterType = typeof(T),
                    Body = predicate.Body,
                    UnionType = unionType,
                    AlisName = alisName,
                    UnionAlisName = alisName
                };
                UnionList.Add(u2);
            }

            ExpressionUnionInfo us = new ExpressionUnionInfo
            {
                Model = typeof(TModel2),
                MasterType = typeof(TModel1),
                Body = predicate.Body,
                UnionType = unionType,
                AlisName = alisName,
                UnionAlisName = unionAlisName
            };
            UnionList.Add(us);

            return this;
        }

        /// <summary>
        ///  执行内连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> InnerJoin<TModel>(string alisName, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, UnionType.INNER_JOIN, predicate);

        /// <summary>
        ///  执行内连接查询
        /// </summary>
        /// <typeparam name="TModel1">要连接的数据库实体对象</typeparam>
        /// <typeparam name="TModel2">要连接的数据库实体对象</typeparam>
        /// <param name="unionAlisName">表连接别名</param>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> InnerJoin<TModel1, TModel2>(string alisName, string unionAlisName, Expression<Func<TModel1, TModel2, bool>> predicate) => Union<TModel1, TModel2>(alisName, unionAlisName, UnionType.INNER_JOIN, predicate);

        /// <summary>
        ///  执行左连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> LeftJoin<TModel>(string alisName, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, UnionType.LEFT_JOIN, predicate);

        /// <summary>
        ///  执行左连接查询
        /// </summary>
        /// <typeparam name="TModel1">要连接的数据库实体对象</typeparam>
        /// <typeparam name="TModel2">要连接的数据库实体对象</typeparam>
        /// <param name="unionAlisName">表连接别名</param>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> LeftJoin<TModel1, TModel2>(string alisName, string unionAlisName, Expression<Func<TModel1, TModel2, bool>> predicate) => Union<TModel1, TModel2>(alisName, unionAlisName, UnionType.LEFT_JOIN, predicate);

        /// <summary>
        ///  执行左外连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> LeftOuterJoin<TModel>(string alisName, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, UnionType.LEFT_OUTER_JOIN, predicate);

        /// <summary>
        ///  执行左外连接查询
        /// </summary>
        /// <typeparam name="TModel1">要连接的数据库实体对象</typeparam>
        /// <typeparam name="TModel2">要连接的数据库实体对象</typeparam>
        /// <param name="unionAlisName">表连接别名</param>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> LeftOuterJoin<TModel1, TModel2>(string alisName, string unionAlisName, Expression<Func<TModel1, TModel2, bool>> predicate) => Union<TModel1, TModel2>(alisName, unionAlisName, UnionType.LEFT_OUTER_JOIN, predicate);

        /// <summary>
        ///  执行右连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> RightJoin<TModel>(string alisName, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, UnionType.RIGHT_JOIN, predicate);

        /// <summary>
        ///  执行右连接查询
        /// </summary>
        /// <typeparam name="TModel1">要连接的数据库实体对象</typeparam>
        /// <typeparam name="TModel2">要连接的数据库实体对象</typeparam>
        /// <param name="unionAlisName">表连接别名</param>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> RightJoin<TModel1, TModel2>(string alisName, string unionAlisName, Expression<Func<TModel1, TModel2, bool>> predicate) => Union<TModel1, TModel2>(alisName, unionAlisName, UnionType.RIGHT_JOIN, predicate);

        /// <summary>
        ///  执行右外连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> RightOuterJoin<TModel>(string alisName, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, UnionType.RIGHT_OUTER_JOIN, predicate);

        /// <summary>
        ///  执行右外连接查询
        /// </summary>
        /// <typeparam name="TModel1">要连接的数据库实体对象</typeparam>
        /// <typeparam name="TModel2">要连接的数据库实体对象</typeparam>
        /// <param name="unionAlisName">表连接别名</param>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public ISelectBuilder<T> RightOuterJoin<TModel1, TModel2>(string alisName, string unionAlisName, Expression<Func<TModel1, TModel2, bool>> predicate) => Union<TModel1, TModel2>(alisName, unionAlisName, UnionType.RIGHT_OUTER_JOIN, predicate);

        #endregion

        /// <summary>
        ///  将查询命令和条件转换为 SQL 语句
        /// </summary>
        /// <returns></returns>
        public override string ToSQL()
        {
            Type mastertype = typeof(T);
            string tableName = MyStagingUtils.GetMapping(mastertype, ProviderType.MySql);
            // master table
            StringBuilder sqlText = new StringBuilder();
            var fields = string.Join(",", Fields);
            sqlText.AppendLine($"SELECT {fields} FROM  {tableName} {masterAlisName}");
            // union
            int _index = 2;
            foreach (var item in UnionList)
            {
                DbExpressionVisitor expression = new DbExpressionVisitor
                {
                    TypeMaster = item.MasterType,
                    AliasMaster = item.AlisName,
                    AliasUnion = item.UnionAlisName
                };
                expression.Visit(item.Body);
                string unionTableName = MyStagingUtils.GetMapping(item.Model, ProviderType.MySql);
                sqlText.AppendLine(item.UnionType.ToString().Replace("_", " ") + " " + unionTableName + " " + expression.AliasUnion + " ON " + expression.SqlText.Builder.ToString());
                foreach (var p in expression.SqlText.Parameters)
                {
                    AddParameter(p.Name, p.Value);
                }
                _index++;
            }
            // condition
            if (WhereExpressions.Count > 0)
            {
                foreach (var item in WhereExpressions)
                {
                    DbExpressionVisitor expression = new DbExpressionVisitor();
                    if (UnionList.Count == 0)
                    {
                        expression.TypeMaster = item.Model;
                        expression.AliasMaster = masterAlisName;
                    }
                    else
                    {
                        ExpressionUnionInfo union = null;
                        if (item.UnionAlisName == null)
                            union = UnionList.FirstOrDefault(f => f.Model == item.Model);
                        else
                            union = UnionList.FirstOrDefault(f => f.Model == item.Model && f.UnionAlisName == item.UnionAlisName);

                        if (union == null && typeof(T) == item.Model)
                        {
                            expression.TypeMaster = item.Model;
                            expression.AliasMaster = masterAlisName;
                        }
                        else if (union != null)
                        {
                            expression.AliasMaster = union.AlisName;
                            expression.AliasUnion = union.UnionAlisName;
                        }
                        else
                        {
                            throw new NotSupportedException($"找不到 where {item.Body}条件的表，不支持的表查询条件");
                        }
                    }
                    expression.Visit(item.Body);
                    WhereConditions.Add(expression.SqlText.Builder.ToString().ToLower());
                    foreach (var p in expression.SqlText.Parameters)
                    {
                        AddParameter(p.Name, p.Value);
                    }
                }
            }

            if (WhereConditions.Count > 0)
                sqlText.AppendLine("WHERE " + string.Join("\nAND ", WhereConditions));
            if (!string.IsNullOrEmpty(GroupByText))
                sqlText.AppendLine(GroupByText);
            if (!string.IsNullOrEmpty(GroupByText) && !string.IsNullOrEmpty(HavingText))
                sqlText.AppendLine(HavingText);
            if (!string.IsNullOrEmpty(OrderByText))
                sqlText.AppendLine(OrderByText);
            if (!string.IsNullOrEmpty(LimitText))
                sqlText.AppendLine(LimitText);

            CommandText = sqlText.ToString();

            return CommandText;
        }

        /// <summary>
        ///  执行查询，并返回受影响的行数
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string cmdText)
        {
            return this.ExecuteNonQuery(cmdText, this.Parameters.ToArray());
        }

        /// <summary>
        ///  执行查询，并返回受影响的行数
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string cmdText, DbParameter[] parameters)
        {
            var affrows = 0;
            try
            {
                affrows = dbContext.Execute.ExecuteNonQuery(CommandType.Text, cmdText, parameters);
            }
            finally
            {
                this.Clear();
            }

            return affrows;
        }

        /// <summary>
        ///  添加仅从已配置的主数据源中查询数据的约束
        /// </summary>
        /// <returns></returns>
        public ISelectBuilder<T> ByMaster()
        {
            this.byMaster = !byMaster;
            return this;
        }

        public new void Clear()
        {
            this.UnionList.Clear();
            this.Parameters.Clear();
            this.WhereConditions.Clear();
            this.WhereExpressions.Clear();
            this.CommandText = null;
            this.Fields.Clear();
            this.LimitText = null;
            this.GroupByText = null;
            this.HavingText = null;
            this.OrderByText = null;
        }

        #region Properties
        /// <summary>
        ///  获取或者设置表连接查询列表
        /// </summary>
        protected List<ExpressionUnionInfo> UnionList { get; set; } = new List<ExpressionUnionInfo>();

        /// <summary>
        ///  获取或者设置查询字段列表
        /// </summary>
        protected List<string> Fields { get; } = new List<string>();

        /// <summary>
        ///  获取或者设置 limit 的值，格式为： limit 10
        /// </summary>
        protected string LimitText { get; set; }

        /// <summary>
        ///  获取或者设置分组条件，格式为：group by xxx,xxx
        /// </summary>
        protected string GroupByText { get; set; }

        /// <summary>
        ///  获取或者设置 hanving 的条件，格式为 having xxx
        /// </summary>
        protected string HavingText { get; set; }

        /// <summary>
        ///  获取或者设置排序条件，格式为：order by xxx asc ,xxx desc
        /// </summary>
        protected string OrderByText { get; set; }

        /// <summary>
        ///  获取或者设置从从库数据源中执行查询，默认值为 false （true 仅从主数据源中读取数据）
        ///  可以针对每一次的查询设置不同的值
        ///  当前 InsertOnReader 方法不受此属性约束
        /// </summary>
        private bool byMaster;

        /// <summary>
        ///  缓存时间，每个子类可重写该成员进行自定义时间设置，单位：seconds
        /// </summary>
        public int Expire { get; set; }

        /// <summary>
        ///  是否进行缓存
        /// </summary>
        public bool Cacheing { get; set; } = true;
        #endregion

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="onExecuted"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public object ExecuteScalarSlave(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            object result = null;
            void Transfer()
            {
                dbContext.Execute.ExecuteScalar(commandType, commandText, commandParameters);
            }

            try
            {
                Transfer();
            }
            catch (System.TimeoutException te)
            {
                dbContext.WriteLog(te);
                Transfer();
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                dbContext.WriteLog(ex);
                Transfer();
            }
            return result;
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="onExecuted"></param>
        /// <param name="parameters"></param>
        public void ExecuteDataReader(Action<DbDataReader> action, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            dbContext.Execute.ExecuteDataReader(action, commandType, commandText, parameters);
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="onExecuted"></param>
        /// <param name="parameters"></param>
        public void ExecuteDataReaderSlave(Action<DbDataReader> action, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            void Transfer(Exception ex)
            {
                dbContext.Execute.ExecuteDataReader(action, commandType, commandText, parameters);
            }

            try
            {
                Transfer(null);
            }
            catch (System.TimeoutException te)
            {
                dbContext.WriteLog(te);
                Transfer(te);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                dbContext.WriteLog(ex);
                Transfer(ex);
            }
        }

        /// <summary>
        ///  封装多个查询结果集，以管道的形式
        /// </summary>
        /// <param name="master">是否在主库执行查询</param>
        /// <param name="contexts">查询上下文对象</param>
        /// <returns></returns>
        public List<List<dynamic>> ExecutePipeLine(bool master, params IQueryPipeLine[] contexts)
        {
            CheckNotNull.NotEmpty(contexts, nameof(contexts));

            StringBuilder sb = new StringBuilder();
            List<DbParameter> parameters = new List<DbParameter>();
            foreach (var ctx in contexts)
            {
                sb.AppendLine(ctx.CommandText);
                sb.Append(";");
                parameters.AddRange(ctx.Parameters);
            }

            var cmdText = sb.ToString();
            int pipeLine = contexts.Length;
            List<List<dynamic>> result = new List<List<dynamic>>();
            SQLExecute execute = master ? dbContext.ByMaster().Execute : dbContext.Execute;
            execute.ExecuteDataReaderPipe(dr =>
             {
                 ExcutePipeResult(contexts, dr, pipeLine, result);

             }, CommandType.Text, cmdText, parameters.ToArray());

            return result;
        }

        public void ExcutePipeResult(IQueryPipeLine[] contexts, DbDataReader dr, int pipeLine, List<List<dynamic>> result)
        {
            for (int i = 0; i < pipeLine; i++)
            {
                List<dynamic> list = new List<dynamic>();
                var ctx = contexts[i];
                while (dr.Read())
                {
                    var obj = ReadObj(dr, ctx.ResultType);
                    list.Add(obj);
                };
                dr.NextResult();
                result.Add(list);
            }
        }

        private object ReadObj(DbDataReader reader, Type type)
        {
            var properties = MyStagingUtils.GetDbFields(type);
            reader.Read();
            var obj = Activator.CreateInstance(type);
            foreach (var pi in properties)
            {
                var value = reader[pi.Name];
                if (value != DBNull.Value)
                    pi.SetValue(obj, value);
            }
            return obj;
        }
    }
}
