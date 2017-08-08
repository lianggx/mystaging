using MyStaging.Common;
using MyStaging.Mapping;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;


namespace MyStaging.Helpers
{
    public class QueryContext<T> where T : class, new()
    {
        public QueryContext<T> Page(int page, int size)
        {
            page = page <= 1 ? 0 : page;
            string limit = page == 0 ? "" : $"\nOFFSET {page * size}";
            LimitText = $"LIMIT {size}{limit}";
            return this;
        }

        public QueryContext<T> GroupBy(string groupByText)
        {
            GroupByText = $"GROUP BY {groupByText}";
            return this;
        }

        public QueryContext<T> OrderBy(string sortText)
        {
            OrderByText = $"ORDER BY {sortText}";
            return this;
        }

        public QueryContext<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            MemberExpression exp = (MemberExpression)keySelector.Body;
            OrderByText = $"ORDER BY {exp.Member.Name}";
            return this;
        }

        public QueryContext<T> OrderBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return OrderBy(keySelector);
        }

        public QueryContext<T> OrderDescing<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            MemberExpression exp = (MemberExpression)keySelector.Body;
            OrderByText = $"ORDER BY {exp.Member.Name} DESC";
            return this;
        }

        public QueryContext<T> OrderDescing<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            return OrderDescing(keySelector);
        }

        public QueryContext<T> Having(string havingText)
        {
            HavingText = $"HAVING {havingText}";
            return this;
        }

        public int Count()
        {
            return ToScalar<int>("COUNT(1)");
        }

        public TResult Max<TResult>(string field)
        {
            return ToScalar<TResult>($"MAX({field})");
        }

        public TResult Max<TResult>(Expression<Func<T, TResult>> selector)
        {
            return Max<T, TResult>(selector);
        }

        public TResult Max<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            MemberExpression exp = (MemberExpression)selector.Body;
            return ToScalar<TResult>($"MAX({exp.Member.Name})");
        }

        public TResult Min<TResult>(string field)
        {
            return ToScalar<TResult>($"MIN({field})");
        }

        public TResult Min<TResult>(Expression<Func<TResult, string>> selector)
        {
            return Min(selector);
        }

        public TResult Min<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            MemberExpression exp = (MemberExpression)selector.Body;
            return ToScalar<TResult>($"MIN({exp.Member.Name})");
        }

        public TResult Sum<TResult>(string field)
        {
            return ToScalar<TResult>($"SUM({field})");
        }

        public TResult Sum<TResult>(Expression<Func<T, TResult>> selector)
        {
            return Sum<T, TResult>(selector);
        }

        public TResult Sum<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            MemberExpression exp = (MemberExpression)selector.Body;
            return ToScalar<TResult>($"SUM({exp.Member.Name})");
        }

        public TResult Avg<TResult>(string field)
        {
            return ToScalar<TResult>($"AVG({field})");
        }

        public TResult Avg<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            MemberExpression exp = (MemberExpression)selector.Body;
            return ToScalar<TResult>($"AVG({exp.Member.Name})");
        }

        public TResult ToScalar<TResult>(string field)
        {
            Fields.Clear();
            Fields.Add(field);
            string cmdText = ToSQLString<TResult>();
            object _val = PgSqlHelper.ExecuteScalar(CommandType.Text, cmdText, this.ParamList.ToArray());

            return (TResult)_val;
        }

        public T ToOne()
        {
            Page(1, 1);
            List<T> list = ToList();
            if (list.Count > 0)
                return list[0];
            else
                return default(T);
        }

        public TResult ToOne<TResult>(string fields)
        {
            if (string.IsNullOrEmpty(fields))
                throw new ArgumentException("参数 fields 必须提供，并以逗号分隔");
            Fields.AddRange(fields.Split(','));
            Page(1, 1);
            List<TResult> list = ToList<TResult>();
            if (list.Count > 0)
                return list[0];
            else
                return default(TResult);
        }

        public T ToOne(params string[] fields)
        {
            foreach (var item in fields)
            {
                Fields.Add(item);
            }
            return this.ToOne();
        }

        public List<T> ToList()
        {
            PropertyInfo[] ps = typeof(T).GetProperties();
            foreach (var item in ps)
            {
                if (item.GetCustomAttribute<ForeignKeyMappingAttribute>() != null)
                    continue;
                string alia = UnionList.Count > 0 ? "a." : "";
                Fields.Add(alia + item.Name);
            }
            return ExecuteReader<T>();
        }

        public List<T> ToList(params string[] fields)
        {
            foreach (var item in fields)
            {
                Fields.Add(item);
            }
            return ExecuteReader<T>();
        }

        public List<TResult> ToList<TResult>()
        {
            return ExecuteReader<TResult>();
        }

        protected List<TResult> ExecuteReader<TResult>()
        {
            ToSQLString<TResult>();
            List<TResult> list = new List<TResult>();

            PgSqlHelper.ExecuteDataReader(dr =>
            {
                TResult obj = DynamicBuilder<TResult>.CreateBuilder(dr).Build(dr);
                list.Add(obj);

            }, CommandType.Text, this.CommandText, this.ParamList.ToArray());

            return list;
        }

        protected T InsertOnReader(string cmdText)
        {
            this.CommandText = cmdText;
            T restult = default(T);
            PgSqlHelper.ExecuteDataReader(dr =>
            {
                restult = DynamicBuilder<T>.CreateBuilder(dr).Build(dr);

            }, CommandType.Text, this.CommandText, this.ParamList.ToArray());

            return restult;
        }

        public QueryContext<T> Where(string expression)
        {
            WhereList.Add(expression);
            return this;
        }

        public QueryContext<T> Where(Expression<Func<T, bool>> predicate)
        {
            return Where<T>(predicate);
        }

        public QueryContext<T> Where<TResult>(Expression<Func<TResult, bool>> predicate)
        {
            ExpressionModel em = new ExpressionModel();
            em.Body = predicate.Body;
            em.Model = typeof(TResult);
            WhereExpressionList.Add(em);
            return this;
        }

        public QueryContext<T> Union<TModel>(string alisName, UnionType unionType, Expression<Func<T, TModel, bool>> predicate)
        {
            Union<T, TModel>(alisName, unionType, predicate);
            return this;
        }
        public QueryContext<T> Union<TModel1, TModel2>(string alisName, UnionType unionType, Expression<Func<TModel1, TModel2, bool>> predicate)
        {
            ExpressionUnionModel us = new ExpressionUnionModel();
            us.Model = typeof(TModel2);
            us.Body = predicate.Body;
            us.UnionType = unionType;
            us.AlisName = alisName;
            UnionList.Add(us);
            return this;
        }

        public string ToSQLString<TResult>()
        {
            Type mastertype = typeof(TResult);
            if (mastertype != typeof(T))
                mastertype = typeof(T);
            string tableName = MyStagingUtils.GetMapping(mastertype);
            // 主表
            StringBuilder sqlText = new StringBuilder();
            string masterAlisName = UnionList.Count > 0 ? "a" : "";
            sqlText.AppendLine($"SELECT {string.Join(",", Fields)} FROM  {tableName} {masterAlisName}");
            // union
            int _index = 2;
            foreach (var item in UnionList)
            {
                PgSqlExpression expression = new PgSqlExpression();
                expression.MasterType = mastertype;
                if (UnionList.Count > 0)
                    expression.Master_AlisName = masterAlisName;
                expression.Union_AlisName = item.AlisName;
                expression.ExpressionCapture(item.Body);
                string unionTableName = MyStagingUtils.GetMapping(item.Model);
                sqlText.AppendLine(item.UnionType.ToString().Replace("_", " ") + " " + unionTableName + " " + expression.Union_AlisName + " ON " + expression.CommandText.ToString());
                ParamList.AddRange(expression.Parameters);
                _index++;
            }
            // condition
            if (WhereExpressionList.Count > 0)
            {
                foreach (var item in WhereExpressionList)
                {
                    PgSqlExpression expression = new PgSqlExpression();
                    if (UnionList.Count == 0)
                    {
                        expression.MasterType = item.Model;
                    }
                    else
                    {
                        ExpressionUnionModel union = UnionList.FirstOrDefault(f => f.Model == item.Model);
                        if (union == null && typeof(T) == item.Model)
                        {
                            expression.MasterType = item.Model;
                            expression.Master_AlisName = "a";
                        }
                        else if (union != null)
                        {
                            //  expression. = item.Model;
                            expression.Union_AlisName = union.AlisName;
                        }
                        else
                        {
                            throw new NotSupportedException($"找不到 where {item.Body.ToString()}条件的表，不支持的表查询条件");
                        }
                    }
                    expression.ExpressionCapture(item.Body);
                    WhereList.Add(expression.CommandText.ToString().ToLower());
                    ParamList.AddRange(expression.Parameters);
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

            this.CommandText = sqlText.ToString();

            return this.CommandText;
        }

        public QueryContext<T> AddParameter(string field, NpgsqlDbType dbType, object value, Type specificType = null)
        {
            NpgsqlParameter p = new NpgsqlParameter(field, dbType);
            if (specificType != null)
                p.SpecificType = specificType;

            p.Value = value;
            ParamList.Add(p);
            return this;
        }

        public int ExecuteNonQuery(string cmdText)
        {
            return PgSqlHelper.ExecuteNonQuery(CommandType.Text, cmdText, ParamList.ToArray());
        }

        #region Properties
        protected List<NpgsqlParameter> ParamList { get; set; } = new List<NpgsqlParameter>();
        protected List<ExpressionUnionModel> UnionList { get; set; } = new List<ExpressionUnionModel>();
        protected List<string> Fields { get; } = new List<string>();
        protected List<ExpressionModel> WhereExpressionList { get; } = new List<ExpressionModel>();
        protected List<string> WhereList { get; set; } = new List<string>();
        protected string LimitText { get; set; }
        protected string GroupByText { get; set; }
        protected string HavingText { get; set; }
        protected string OrderByText { get; set; }
        protected string CommandText { get; set; }
        #endregion
    }
}
