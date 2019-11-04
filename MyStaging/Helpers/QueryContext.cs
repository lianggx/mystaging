using MyStaging.Common;
using MyStaging.Interface;
using MyStaging.Mapping;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  数据库查询上下文对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryContext<T> : IQueryPipe where T : class, new()
    {
        private const string masterAlisName = "a";

        /// <summary>
        ///  设置当前查询的 limit 和 offset
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public QueryContext<T> Page(int page, int size)
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
        public QueryContext<T> GroupBy(string groupByText)
        {
            GroupByText = $"GROUP BY {groupByText}";
            return this;
        }

        /// <summary>
        ///  设置查询的排序条件，格式为：字段 ASC，如果有多个排序，以逗号分隔
        /// </summary>
        /// <param name="sortText"></param>
        /// <returns></returns>
        public QueryContext<T> OrderBy(string sortText)
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
        public QueryContext<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector) => OrderTransafer(keySelector);

        /// <summary>
        ///  设置查询的排序条件，该方法将根据表达式自动推导出排序的字段，排序方向为 ASC
        /// </summary>
        /// <typeparam name="TSource">排序字段对象</typeparam>
        /// <typeparam name="TKey">字段</typeparam>
        /// <param name="keySelector">字段选择器</param>
        /// <returns></returns>
        public QueryContext<T> OrderBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector) => OrderTransafer(keySelector);

        /// <summary>
        ///  设置查询的排序条件，该方法将根据表达式自动推导出排序的字段，排序方向为 DESC
        /// </summary>
        /// <typeparam name="TKey">字段</typeparam>
        /// <param name="keySelector">字段选择器</param>
        /// <returns></returns>
        public QueryContext<T> OrderByDescing<TKey>(Expression<Func<T, TKey>> keySelector) => OrderTransafer(keySelector.Body, "DESC");

        /// <summary>
        ///  设置查询的排序条件，该方法将根据表达式自动推导出排序的字段，排序方向为 DESC
        /// </summary>
        /// <typeparam name="TSource">排序字段对象</typeparam>
        /// <typeparam name="TKey">字段</typeparam>
        /// <param name="keySelector">字段选择器</param>
        /// <returns></returns>
        public QueryContext<T> OrderByDescing<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector) => OrderTransafer(keySelector.Body, "DESC");

        /// <summary>
        ///  设置查询排序条件和方向
        /// </summary>
        /// <param name="keySelector">字段选择器</param>
        /// <param name="direction">排序方向，默认 ASC</param>
        /// <returns></returns>
        private QueryContext<T> OrderTransafer(Expression keySelector, string direction = "ASC")
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
        public QueryContext<T> Having(string havingText)
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
        public TResult Max<TResult>(string field) => ToScalar<TResult>($"COALESCE(MAX({field}),0)");

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
        public TResult Max<TSource, TResult>(Expression<Func<TSource, TResult>> selector) => Max<TResult>(GetMemberName(selector));

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
        public TResult Min<TSource, TResult>(Expression<Func<TSource, TResult>> selector) => Min<TResult>(GetMemberName(selector));

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
        public TResult Sum<TSource, TResult>(Expression<Func<TSource, TResult>> selector) => Sum<TResult>(GetMemberName(selector));

        /// <summary>
        ///  获取表达式成员名称
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        private string GetMemberName<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            MemberExpression exp = null;
            if (selector.Body.NodeType == ExpressionType.Convert)
            {
                exp = (MemberExpression)((UnaryExpression)selector.Body).Operand;
            }
            else
                exp = (MemberExpression)selector.Body;

            return exp.Member.Name;
        }

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
        public TResult Avg<TSource, TResult>(Expression<Func<TSource, TResult>> selector) => Avg<TResult>(GetMemberName(selector));

        /// <summary>
        ///  查询返回第一行第一列字段的值
        /// </summary>
        /// <typeparam name="TResult">接受查询结果类型</typeparam>
        /// <param name="field">查询的字段</param>
        /// <returns></returns>
        public TResult ToScalar<TResult>(string field)
        {
            Fields.Clear();
            Fields.Add(field);
            string cmdText = ToSQL();
            object result = null;
            if (Master)
            {
                result = PgSqlHelper.ExecuteScalar(CommandType.Text, cmdText, command =>
                {
                    this.Command = command;
                    this.Connection = command?.Connection;
                }, this.ParamList.ToArray());
            }
            else
            {
                result = PgSqlHelper.ExecuteScalarSlave(CommandType.Text, cmdText, command =>
                {
                    this.Command = command;
                    this.Connection = command?.Connection;
                }, this.ParamList.ToArray());
            }
            Clear();
            return (TResult)result;
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
            var enable = Cacheing && PgSqlHelper.CacheManager != null && objType.IsClass;
            // 读取缓存
            TResult obj = default(TResult);
            if (enable && cacheing)
            {
                obj = PgSqlHelper.CacheManager.GetItemCache<TResult>(this.ParamList);
                if (obj != null)
                {
                    this.Clear();
                    return obj;
                }
            }

            List<TResult> list = ExecuteReader<TResult>(this.commandtext);

            if (list.Count > 0)
            {
                var result = list[0];
                if (enable)
                {
                    PgSqlHelper.CacheManager.SetItemCache(result, this.Expire);
                }

                return result;
            }
            else
                return default(TResult);
        }

        /// <summary>
        ///  查询返回一行数据
        /// </summary>
        /// <typeparam name="TResult">接受查询结果对象类型</typeparam>
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
            return ExecuteReader<TResult>(this.commandtext);
        }

        public IQueryPipe ToPipe(int page = 1, int size = 10, params string[] fields) => ToPipe<T>(page, size, fields);

        public IQueryPipe ToPipe<TResult>(int page = 1, int size = 10, params string[] fields)
        {
            this.Page(page, size);
            ResetFields(fields);
            PipeResultType = typeof(TResult);
            return this;
        }

        public Type PipeResultType { get; set; }

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
            DynamicBuilder<TResult> builder = null;
            Type objType = typeof(TResult);
            try
            {
                if (Master)
                {
                    PgSqlHelper.ExecuteDataReader(dr =>
                    {
                        var obj = ReadObj<TResult>(ref builder, dr, objType);
                        list.Add(obj);
                    }
                    , CommandType.Text, cmdText, command =>
                    {
                        this.Command = command;
                        this.Connection = command?.Connection;
                    }, this.ParamList.ToArray());
                }
                else
                {
                    PgSqlHelper.ExecuteDataReaderSlave(dr =>
                    {
                        var obj = ReadObj<TResult>(ref builder, dr, objType);
                        list.Add(obj);
                    }
                    , CommandType.Text, cmdText, command =>
                     {
                         this.Command = command;
                         this.Connection = command?.Connection;
                     }, this.ParamList.ToArray());
                }
            }
            finally
            {
                this.Clear();
            }
            return list;
        }

        private bool IsTuple(Type type) => type.Namespace == "System" && type.Name.StartsWith("ValueTuple`");

        public TResult ReadObj<TResult>(ref DynamicBuilder<TResult> builder, DbDataReader dr, Type objType)
        {
            TResult obj = default(TResult);
            bool isTuple = IsTuple(objType);
            bool isEnum = objType.IsEnum;
            if (isTuple)
            {
                int columnIndex = -1;
                obj = (TResult)GetValueTuple(objType, dr, ref columnIndex);
            }
            else if (IsValueType(objType) || isEnum)
            {
                obj = (TResult)GetValueType(objType, dr);
            }
            else if (objType.Namespace != null && objType.Namespace.StartsWith("Newtonsoft"))
            {
                obj = (TResult)GetJToken(dr);
            }
            else
            {
                if (builder == null)
                {
                    builder = DynamicBuilder<TResult>.CreateBuilder(dr);
                }
                obj = builder.Build(dr);
            }
            return obj;
        }

        public object ReadObj(DynamicBuilder builder, DbDataReader dr, Type objType)
        {
            object obj = null;
            bool isTuple = IsTuple(objType);
            bool isEnum = objType.IsEnum;
            if (isTuple)
            {
                int columnIndex = -1;
                obj = GetValueTuple(objType, dr, ref columnIndex);
            }
            else if (IsValueType(objType) || isEnum)
            {
                obj = GetValueType(objType, dr);
            }
            else if (objType.Namespace != null && objType.Namespace.StartsWith("Newtonsoft"))
            {
                obj = GetJToken(dr);
            }
            else
            {
                obj = builder.Build(dr);
            }

            return obj;
        }

        /// <summary>
        ///  检查查询结果对象是否为元组类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected bool IsValueType(Type type)
        {
            return (type.Namespace == "System" && type.Name.StartsWith("String")) || (type.BaseType == typeof(ValueType));
        }

        /// <summary>
        ///  将查询结果转换为元组对象
        /// </summary>
        /// <param name="objType">元组类型</param>
        /// <param name="dr">查询流</param>
        /// <param name="columnIndex">dr index</param>
        /// <returns></returns>
        protected object GetValueTuple(Type objType, IDataReader dr, ref int columnIndex)
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
        ///  从数据库流中读取值并转换为指定的对象类型
        /// </summary>
        /// <param name="objType">对象类型</param>
        /// <param name="dr">查询流</param>
        /// <returns></returns>
        protected object GetValueType(Type objType, IDataReader dr)
        {
            object dbValue = dr[0];
            dbValue = dbValue is DBNull ? null : dbValue;
            dbValue = Convert.ChangeType(dbValue, objType);

            return dbValue;
        }

        /// <summary>
        ///  将查询结果转换为 JToken 对象
        /// </summary>
        /// <param name="dr">查询流</param>
        /// <returns></returns>
        protected object GetJToken(IDataReader dr)
        {
            object dbValue = dr[0];
            if (dbValue is DBNull)
                return null;
            else
                return JToken.Parse(dbValue.ToString());
        }

        /// <summary>
        ///  执行 Insert 语句并返回 Insert 后的结果,该方法仅提供在主数据源上执行，无法对从库源执行此方法
        /// </summary>
        /// <param name="cmdText">Insert SQL 语句</param>
        /// <returns></returns>
        protected T InsertOnReader(string cmdText)
        {
            this.commandtext = cmdText;
            T restult = default(T);
            try
            {
                PgSqlHelper.ExecuteDataReader(dr =>
                {
                    restult = DynamicBuilder<T>.CreateBuilder(dr).Build(dr);
                }, CommandType.Text, this.commandtext, this.ParamList.ToArray());
            }
            finally
            {
                this.Clear();
            }
            return restult;
        }

        /// <summary>
        ///  该方法没有对sql注入进行参数化过滤
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public QueryContext<T> Where(string expression)
        {
            if (string.IsNullOrEmpty(expression)) throw new ArgumentNullException("必须传递参数 expression");

            WhereList.Add($"({expression})");
            return this;
        }

        /// <summary>
        ///  增加查询条件
        /// </summary>
        /// <param name="formatCommad">格式为{0}{1}的字符串</param>
        /// <param name="pValue">{0}{1}对应的值</param>
        /// <returns></returns>
        public QueryContext<T> Where(string formatCommad, params object[] pValue)
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
            return this.Where(expression);
        }

        /// <summary>
        ///  增加查询条件
        /// </summary>
        /// <param name="predicate">查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> Where(Expression<Func<T, bool>> predicate) => Where<T>(null, predicate);

        /// <summary>
        ///  增加查询条件
        /// </summary>
        /// <typeparam name="TResult">查询表达式的对象</typeparam>
        /// <param name="predicate">查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> Where<TResult>(Expression<Func<TResult, bool>> predicate) => this.Where<TResult>(null, predicate);

        /// <summary>
        ///  增加查询条件
        /// </summary>
        /// <typeparam name="TResult">查询表达式的对象</typeparam>
        /// <param name="alisName">alisName</param>
        /// <param name="predicate">查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> Where<TResult>(string alisName, Expression<Func<TResult, bool>> predicate)
        {
            ExpressionModel em = new ExpressionModel
            {
                Body = predicate.Body,
                Model = typeof(TResult),
                UnionAlisName = alisName
            };
            WhereExpressionList.Add(em);
            return this;
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
        public QueryContext<T> Union<TModel>(string alisName, UnionType unionType, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, unionType, predicate);

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
        public QueryContext<T> Union<TModel1, TModel2>(string alisName, string unionAlisName, UnionType unionType, Expression<Func<TModel1, TModel2, bool>> predicate)
        {
            Type type = typeof(TModel1);
            var last = UnionList.Where(f => f.Model.Equals(type) && f.UnionAlisName == alisName).FirstOrDefault();

            if (alisName != masterAlisName && last == null)
            {
                ExpressionUnionModel u2 = new ExpressionUnionModel
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

            ExpressionUnionModel us = new ExpressionUnionModel
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
        public QueryContext<T> InnerJoin<TModel>(string alisName, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, UnionType.INNER_JOIN, predicate);

        /// <summary>
        ///  执行内连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> InnerJoin<TModel1, TModel2>(string alisName, string unionAlisName, Expression<Func<TModel1, TModel2, bool>> predicate) => Union<TModel1, TModel2>(alisName, unionAlisName, UnionType.INNER_JOIN, predicate);

        /// <summary>
        ///  执行左连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> LeftJoin<TModel>(string alisName, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, UnionType.LEFT_JOIN, predicate);

        /// <summary>
        ///  执行左连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> LeftJoin<TModel1, TModel2>(string alisName, string unionAlisName, Expression<Func<TModel1, TModel2, bool>> predicate) => Union<TModel1, TModel2>(alisName, unionAlisName, UnionType.LEFT_JOIN, predicate);

        /// <summary>
        ///  执行左外连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> LeftOuterJoin<TModel>(string alisName, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, UnionType.LEFT_OUTER_JOIN, predicate);

        /// <summary>
        ///  执行左外连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> LeftOuterJoin<TModel1, TModel2>(string alisName, string unionAlisName, Expression<Func<TModel1, TModel2, bool>> predicate) => Union<TModel1, TModel2>(alisName, unionAlisName, UnionType.LEFT_OUTER_JOIN, predicate);

        /// <summary>
        ///  执行右连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> RightJoin<TModel>(string alisName, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, UnionType.RIGHT_JOIN, predicate);

        /// <summary>
        ///  执行右连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> RightJoin<TModel1, TModel2>(string alisName, string unionAlisName, Expression<Func<TModel1, TModel2, bool>> predicate) => Union<TModel1, TModel2>(alisName, unionAlisName, UnionType.RIGHT_JOIN, predicate);

        /// <summary>
        ///  执行右外连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> RightOuterJoin<TModel>(string alisName, Expression<Func<T, TModel, bool>> predicate) => Union<T, TModel>("a", alisName, UnionType.RIGHT_OUTER_JOIN, predicate);

        /// <summary>
        ///  执行右外连接查询
        /// </summary>
        /// <typeparam name="TModel">要连接的数据库实体对象</typeparam>
        /// <param name="alisName">连接的别名</param>
        /// <param name="predicate">On 的查询表达式</param>
        /// <returns></returns>
        public QueryContext<T> RightOuterJoin<TModel1, TModel2>(string alisName, string unionAlisName, Expression<Func<TModel1, TModel2, bool>> predicate) => Union<TModel1, TModel2>(alisName, unionAlisName, UnionType.RIGHT_OUTER_JOIN, predicate);

        #endregion

        /// <summary>
        ///  将查询命令和条件转换为 SQL 语句
        /// </summary>
        /// <returns></returns>
        public string ToSQL()
        {
            Type mastertype = typeof(T);
            string tableName = MyStagingUtils.GetMapping(mastertype);
            // master table
            StringBuilder sqlText = new StringBuilder();
            sqlText.AppendLine($"SELECT {string.Join(",", Fields)} FROM  {tableName} {masterAlisName}");
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
                string unionTableName = MyStagingUtils.GetMapping(item.Model);
                sqlText.AppendLine(item.UnionType.ToString().Replace("_", " ") + " " + unionTableName + " " + expression.AliasUnion + " ON " + expression.SqlText.Builder.ToString());
                ParamList.AddRange(expression.SqlText.Parameters);
                _index++;
            }
            // condition
            if (WhereExpressionList.Count > 0)
            {
                foreach (var item in WhereExpressionList)
                {
                    DbExpressionVisitor expression = new DbExpressionVisitor();
                    if (UnionList.Count == 0)
                    {
                        expression.TypeMaster = item.Model;
                        expression.AliasMaster = masterAlisName;
                    }
                    else
                    {
                        ExpressionUnionModel union = null;
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
                            throw new NotSupportedException($"找不到 where {item.Body.ToString()}条件的表，不支持的表查询条件");
                        }
                    }
                    expression.Visit(item.Body);
                    WhereList.Add(expression.SqlText.Builder.ToString().ToLower());
                    ParamList.AddRange(expression.SqlText.Parameters);
                }
            }

            if (WhereList.Count > 0)
                sqlText.AppendLine("WHERE " + string.Join("\nAND ", WhereList));
            if (!string.IsNullOrEmpty(GroupByText))
                sqlText.AppendLine(GroupByText);
            if (!string.IsNullOrEmpty(GroupByText) && !string.IsNullOrEmpty(HavingText))
                sqlText.AppendLine(HavingText);
            if (!string.IsNullOrEmpty(OrderByText))
                sqlText.AppendLine(OrderByText);
            if (!string.IsNullOrEmpty(LimitText))
                sqlText.AppendLine(LimitText);

            this.commandtext = sqlText.ToString();

            return this.commandtext;
        }

        /// <summary>
        ///  增加一个查询参数
        /// </summary>
        /// <param name="field">数据库字段</param>
        /// <param name="value">字段指定的值</param>
        /// <returns></returns>
        public QueryContext<T> AddParameter(string field, object value)
        {
            NpgsqlParameter p = new NpgsqlParameter(field, value);
            ParamList.Add(p);
            return this;
        }

        /// <summary>
        ///  增加一个查询参数
        /// </summary>
        /// <param name="field">数据库字段</param>
        /// <param name="value">字段指定的值</param>
        /// <param name="size">字段长度</param>
        /// <returns></returns>
        public QueryContext<T> AddParameter(string field, object value, int size)
        {
            return AddParameter(field, null, value, size);
        }

        /// <summary>
        ///  增加一个查询参数
        /// </summary>
        /// <param name="field">数据库字段</param>
        /// <param name="dbType">字段类型</param>
        /// <param name="value">字段指定的值</param>
        /// <returns></returns>
        public QueryContext<T> AddParameter(string field, NpgsqlDbType dbType, object value) => AddParameter(field, dbType, value, -1);

        /// <summary>
        ///  增加一个查询参数
        /// </summary>
        /// <param name="field">数据库字段</param>
        /// <param name="dbType">字段类型</param>
        /// <param name="value">字段指定的值</param>
        /// <param name="size">字段长度</param>
        /// <returns></returns>
        public QueryContext<T> AddParameter(string field, NpgsqlDbType? dbType, object value, int size)
        {
            NpgsqlParameter p = this.ParamList.FirstOrDefault(f => f.ParameterName == field);
            if (p != null)
            {
                this.ParamList.Remove(p);
            }
            if ((dbType == NpgsqlDbType.Json || dbType == NpgsqlDbType.Jsonb) && value != null)
            {
                JToken token = value as JToken;
                if (!token.HasValues)
                {
                    value = null;
                }
            }
            if (dbType.HasValue)
            {
                p = new NpgsqlParameter(field, dbType);
            }
            else
            {
                p = new NpgsqlParameter(field, value);
            }

            if (size != -1)
                p.Size = size;

            p.Value = value;
            ParamList.Add(p);
            return this;
        }

        /// <summary>
        /// 增加一组查询参数
        /// </summary>
        /// <param name="parameters">输入参数</param>
        /// <returns></returns>
        public QueryContext<T> AddParameter(NpgsqlParameter[] parameters)
        {
            CheckNotNull.NotEmpty(parameters, nameof(parameters));

            ParamList.AddRange(parameters);
            return this;
        }

        /// <summary>
        /// 增加一个查询参数
        /// </summary>
        /// <param name="parameters">输入参数</param>
        /// <returns></returns>
        public QueryContext<T> AddParameter(NpgsqlParameter parameter)
        {
            CheckNotNull.NotNull(parameter, nameof(parameter));

            ParamList.Add(parameter);
            return this;
        }

        /// <summary>
        ///  将数组对象拼接成 array[''] 使用的条件
        /// </summary>
        /// <param name="items">待拼接的数组</param>
        /// <param name="dbtype">数据库字段类型</param>
        /// <param name="enumtype">指定的枚举类型</param>
        /// <returns></returns>
        protected string JoinTo(System.Collections.ICollection items, string type)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var item in items)
            {
                string pName = Guid.NewGuid().ToString("N");
                AddParameter(pName, item.ToString());
                sb.Append("@" + pName + "::" + type);
                if (++i < items.Count)
                    sb.Append(",");
            }
            return sb.ToString();
        }

        /// <summary>
        ///  执行查询，并返回受影响的行数
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string cmdText)
        {
            return this.ExecuteNonQuery(cmdText, this.ParamList.ToArray());
        }

        /// <summary>
        ///  执行查询，并返回受影响的行数
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string cmdText, DbParameter[] parameters)
        {
            var affrows = 0;
            try
            {
                affrows = PgSqlHelper.ExecuteNonQuery(CommandType.Text, cmdText, command =>
                {
                    this.Command = command;
                    this.Connection = command?.Connection;
                }, parameters);
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
        public QueryContext<T> ByMaster()
        {
            this.Master = true;
            return this;
        }

        /// <summary>
        ///  清除参数列表
        /// </summary>
        protected void Clear()
        {
            this.ParamList.Clear();
            this.WhereList.Clear();
        }

        #region Properties
        public DbCommand Command { get; internal set; }
        public DbConnection Connection { get; internal set; }
        /// <summary>
        ///  获取或者设置参数列表
        /// </summary>
        public List<NpgsqlParameter> ParamList { get; set; } = new List<NpgsqlParameter>();

        /// <summary>
        ///  获取或者设置表连接查询列表
        /// </summary>
        protected List<ExpressionUnionModel> UnionList { get; set; } = new List<ExpressionUnionModel>();

        /// <summary>
        ///  获取或者设置查询字段列表
        /// </summary>
        protected List<string> Fields { get; } = new List<string>();

        /// <summary>
        ///  获取或者设置查询表达式列表
        /// </summary>
        protected List<ExpressionModel> WhereExpressionList { get; } = new List<ExpressionModel>();

        /// <summary>
        ///  获取或者设置查询条件列表
        /// </summary>
        protected List<string> WhereList { get; set; } = new List<string>();

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

        private string commandtext = string.Empty;
        /// <summary>
        ///  获取或者设置查询语句，设置该属性将覆盖所有的查询条件
        /// </summary>
        public string CommandText { get { return commandtext; } set { this.commandtext = value; } }

        /// <summary>
        ///  获取或者设置从从库数据源中执行查询，默认值为 false （true 仅从主数据源中读取数据）
        ///  可以针对每一次的查询设置不同的值
        ///  当前 InsertOnReader 方法不受此属性约束
        /// </summary>
        public bool Master { get; set; } = false;

        /// <summary>
        ///  缓存时间，每个子类可重写该成员进行自定义时间设置，单位：seconds
        /// </summary>
        public virtual int Expire { get; set; }

        /// <summary>
        ///  是否进行缓存
        /// </summary>
        public virtual bool Cacheing { get; set; } = true;
        #endregion
    }
}
