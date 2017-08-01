using MyStaging.Common;
using MyStaging.Helpers;
using Npgsql;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;


namespace MyStaging.Helpers
{
    /// <summary>
    ///  SQL Query Context
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SqlQueryContext<T> : IDAL where T : class, new()
    {
        /// <summary>
        ///  Paging
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public SqlQueryContext<T> Page(int page, int size)
        {
            page = page <= 1 ? 0 : page;
            LimitText = $"LIMIT {size}  \nOFFSET {page * size}";
            return this;
        }

        /// <summary>
        ///  group by
        /// </summary>
        /// <param name="sortText">Example: a.create_time ,b.update_time </param>
        /// <returns></returns>
        public SqlQueryContext<T> GroupBy(string groupByText)
        {
            GroupByText = $"GROUP BY {groupByText}";
            return this;
        }

        /// <summary>
        ///  order by
        /// </summary>
        /// <param name="sortText">fields，Example: create_time desc,update_time asc</param>
        /// <returns></returns>
        public SqlQueryContext<T> OrderBy(string sortText)
        {
            OrderByText = $"ORDER BY {sortText}";
            return this;
        }

        /// <summary>
        ///  having
        /// </summary>
        /// <param name="sortText">having condition，Example: count(name)>2</param>
        /// <returns></returns>
        public SqlQueryContext<T> Having(string havingText)
        {
            HavingText = $"HAVING {havingText}";
            return this;
        }

        /// <summary>
        ///  get record total
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return ToScalar<int>("COUNT(1)");
        }

        /// <summary>
        ///  max
        /// </summary>
        /// <returns></returns>
        public decimal Max(string field)
        {
            return ToScalar<int>($"MAX({field})");
        }

        /// <summary>
        ///  min
        /// </summary>
        /// <returns></returns>
        public decimal Min(string field)
        {
            return ToScalar<int>($"MIN({field})");
        }

        /// <summary>
        ///  sum
        /// </summary>
        /// <returns></returns>
        public decimal Sum(string field)
        {
            return ToScalar<int>($"SUM({field})");
        }

        /// <summary>
        ///  avg
        /// </summary>
        /// <returns></returns>
        public decimal Avg(string field)
        {
            return ToScalar<int>($"AVG({field})");
        }

        /// <summary>
        ///  scalar
        /// </summary>
        /// <typeparam name="TResult">return type</typeparam>
        /// <param name="field">use field or db function，Example: MAX(age) </param>
        /// <returns></returns>
        public TResult ToScalar<TResult>(string field)
        {
            Fields.Clear();
            Fields.Add(field);
            string cmdText = ToSQLString();
            object _val = PgSqlHelper.ExecuteScalar(CommandType.Text, cmdText, this.ParamList.ToArray());

            return (TResult)_val;
        }

        /// <summary>
        ///  get first record
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ToOne()
        {
            Page(1, 1);
            List<T> list = ToList();
            if (list.Count > 0)
                return list[0];
            else
                return default(T);
        }

        /// <summary>
        ///  get first record
        /// </summary>
        /// <param name="fields">fields，Example: a.id,b.name,c.age</param>
        /// <returns></returns>
        public T ToOne(params string[] fields)
        {
            foreach (var item in fields)
            {
                Fields.Add(item);
            }
            return this.ToOne();
        }

        /// <summary>
        ///  get range
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            PropertyInfo[] pis = typeof(T).GetProperties();
            string cmdText = ToSQLString(pis);
            List<T> list = ExecuteReader(cmdText, pis);
            return list;
        }

        private List<T> ExecuteReader(string cmdText, PropertyInfo[] pis)
        {
            List<T> list = new List<T>();
            PgSqlHelper.ExecuteDataReader(dr =>
            {
                IDictionary<string, string> drNameList = new Dictionary<string, string>();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string dr_name = dr.GetName(i);
                    drNameList[dr_name.ToLower()] = dr_name;
                }
                T result = new T();
                for (int i = 0; i < pis.Length; i++)
                {
                    PropertyInfo pi = pis[i];
                    string dr_name = null;
                    string pi_name = pi.Name.ToLower();
                    if (drNameList.ContainsKey(pi_name) == false)
                        continue;
                    else
                        dr_name = drNameList[pi_name];
                    object rValue = dr[dr_name];
                    if (rValue != null)
                        pi.SetValue(result, rValue, null);
                }
                list.Add(result);

            }, CommandType.Text, cmdText, this.ParamList.ToArray());

            return list;
        }

        /// <summary>
        ///  get range
        /// </summary>
        /// <param name="fields">fields，Example: a.id,b.name,c.age</param>
        /// <returns></returns>
        public List<T> ToList(params string[] fields)
        {
            foreach (var item in fields)
            {
                Fields.Add(item);
            }
            return this.ToList();
        }

        /// <summary>
        ///  add where condition
        /// </summary>
        /// <param name="field">field name Example: id , a.id</param>
        /// <param name="type">field in NpgsqlDbType</param>
        /// <param name="value">parameter value</param>
        /// <returns></returns>
        public SqlQueryContext<T> SetParameter(string field, NpgsqlDbType type, object value, ConditionType wt = ConditionType.Equal)
        {
            NpgsqlParameter _p = new NpgsqlParameter(field, value);
            _p.NpgsqlDbType = type;
            string expression = string.Empty;
            switch (wt)
            {
                default:
                case
                ConditionType.Equal:
                    expression = $"{field}=@{field}";
                    break;
                case ConditionType.NotEqual: expression = $"{field}!=@{field}"; break;
                case ConditionType.Greater: expression = $"{field}>@{field}"; break;
                case ConditionType.Gt: expression = $"{field}>=@{field}"; break;
                case ConditionType.Less: expression = $"{field}<@{field}"; break;
                case ConditionType.Lt: expression = $"{field}<=@{field}"; break;
                case ConditionType.Is_Null: expression = $"{field} IS NULL"; break;
                case ConditionType.Is_Not_Null: expression = $"{field} IS NOT NULL"; break;
            }
            WhereList.Add(expression);
            ParamList.Add(_p);
            return this;
        }

        /// <summary>
        ///  conditaion
        /// </summary>
        /// <param name="sqlExpression">Example: id='1001' and (gender='男' or age > 20)</param>
        /// <returns></returns>
        protected SqlQueryContext<T> Where(string sqlExpression)
        {
            WhereList.Add(sqlExpression);
            return this;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <returns></returns>
        private string ToSQLString(IEnumerable<PropertyInfo> pis)
        {
            if (Fields.Count == 0)
            {
                foreach (var item in pis)
                {
                    Fields.Add("a." + item.Name.ToLower());
                }
                if (JoinList.Count > 0)
                {
                    foreach (var item in JoinList)
                    {
                        Fields.AddRange(item.Fields);
                    }
                }
            }

            return ToSQLString();
        }

        /// <summary>
        ///  
        /// </summary>
        /// <returns></returns>
        public string ToSQLString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT");
            sb.AppendLine(string.Join(",", Fields));
            sb.AppendLine("FROM");
            sb.AppendLine(TableName + " a");
            foreach (var item in JoinList)
            {
                sb.AppendLine($"{item.JoinType.ToString().Replace('_', ' ')} {item.Table} {item.Alias} ON {item.On}");
            }
            if (WhereList.Count > 0)
                sb.AppendLine("WHERE " + string.Join(" AND ", WhereList));
            if (!string.IsNullOrEmpty(GroupByText))
                sb.AppendLine(GroupByText);
            if (!string.IsNullOrEmpty(GroupByText) && !string.IsNullOrEmpty(HavingText))
                sb.AppendLine(HavingText);
            if (!string.IsNullOrEmpty(OrderByText))
                sb.AppendLine(OrderByText);
            if (!string.IsNullOrEmpty(LimitText))
                sb.AppendLine(LimitText);

            return sb.ToString();
        }

        #region execute
        public int ExecuteNonQuery(string cmdText)
        {
            return PgSqlHelper.ExecuteNonQuery(CommandType.Text, cmdText, ParamList.ToArray());
        }

        public T InsertOnReader(string cmdText)
        {
            PropertyInfo[] pis = typeof(T).GetProperties();
            List<T> list = ExecuteReader(cmdText, pis);
            T restult = list.Count == 0 ? default(T) : list[0];
            return restult;
        }
        #endregion

        #region Properties
        /// <summary>
        ///  the current command parameter
        /// </summary>
        public List<NpgsqlParameter> ParamList { get; set; } = new List<NpgsqlParameter>();
        /// <summary>
        ///  specify fields in current command 
        /// </summary>
        protected List<string> Fields { get; } = new List<string>();
        /// <summary>
        ///  conditaion set
        /// </summary>
        protected List<string> WhereList { get; } = new List<string>();
        protected string LimitText { get; set; }
        protected string GroupByText { get; set; }
        protected string HavingText { get; set; }
        protected string OrderByText { get; set; }
        protected List<UnionModel> JoinList { get; set; } = new List<UnionModel>();
        /// <summary>
        ///  execute command datatable fullname ,Example : schema.table
        /// </summary>
        public abstract string TableName { get; }
        /// <summary>
        ///  datatable to entitymodel
        /// </summary>
        public abstract object DbModel { get; }
        #endregion
    }
}
