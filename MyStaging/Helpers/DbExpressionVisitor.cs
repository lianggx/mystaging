using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  lambda 表达式数据库查询转换对象
    /// </summary>
    public class DbExpressionVisitor : ExpressionVisitor
    {
        #region Identity        
        /// <summary>
        ///  获取或者设置 Sql 查询生成实体对象
        /// </summary>
        public SqlTextEntity SqlText { get; set; } = new SqlTextEntity();

        /// <summary>
        /// 获取或者设置当前表达式的主对象类型的别名
        /// </summary>
        public string AliasMaster { get; set; }

        /// <summary>
        ///  获取或者设置当前表达式的连接查询对象类型
        /// </summary>
        public string AliasUnion { get; set; }

        /// <summary>
        /// 获取或者设置当前表达式的主对象类型，用于生成数据库关系对象名称
        /// </summary>
        public Type TypeMaster { get; set; }

        /// <summary>
        ///  获取或者左侧表达式
        /// </summary>
        private Expression Left { get; set; }

        /// <summary>
        ///  获取或者设置右侧表达式
        /// </summary>
        private Expression Right { get; set; }

        /// <summary>
        /// 表达式操作符转换集
        /// </summary>
        protected static Dictionary<ExpressionType, string> OPERATOR_SET = new Dictionary<ExpressionType, string>()
        {
            { ExpressionType.And," & "},
            { ExpressionType.AndAlso," AND "},
            { ExpressionType.Equal," = "},
            { ExpressionType.GreaterThan," > "},
            { ExpressionType.GreaterThanOrEqual," >= "},
            { ExpressionType.LessThan," < "},
            { ExpressionType.LessThanOrEqual," <= "},
            { ExpressionType.NotEqual," <> "},
            { ExpressionType.OrElse," OR "},
            { ExpressionType.Or," | "},
            { ExpressionType.Add," + "},
            { ExpressionType.Subtract," - "},
            { ExpressionType.Divide," / "},
            { ExpressionType.Multiply," * "},
            { ExpressionType.Not," NOT "}

        };

        /// <summary>
        ///  pgsql 数据库类型转换集
        /// </summary>
        protected static Dictionary<Type, NpgsqlDbType> PG_TYPES = new Dictionary<Type, NpgsqlDbType>
            {
                 { typeof(bool[]), NpgsqlDbType.Boolean | NpgsqlDbType.Array },
                 { typeof(short[]), NpgsqlDbType.Smallint | NpgsqlDbType.Array },
                 { typeof(ushort[]), NpgsqlDbType.Smallint | NpgsqlDbType.Array },
                 { typeof(int[]), NpgsqlDbType.Integer | NpgsqlDbType.Array },
                 { typeof(uint[]), NpgsqlDbType.Integer | NpgsqlDbType.Array },
                 { typeof(long[]), NpgsqlDbType.Bigint | NpgsqlDbType.Array },
                 { typeof(ulong[]), NpgsqlDbType.Bigint | NpgsqlDbType.Array },
                 { typeof(float[]), NpgsqlDbType.Real | NpgsqlDbType.Array },
                 { typeof(decimal[]), NpgsqlDbType.Numeric | NpgsqlDbType.Array },
                 { typeof(double[]), NpgsqlDbType.Numeric | NpgsqlDbType.Array },
                 { typeof(char[]), NpgsqlDbType.Varchar | NpgsqlDbType.Array },
                 { typeof(string[]), NpgsqlDbType.Varchar | NpgsqlDbType.Array },
                 { typeof(Guid[]), NpgsqlDbType.Uuid | NpgsqlDbType.Array },
                 { typeof(TimeSpan[]), NpgsqlDbType.Interval | NpgsqlDbType.Array },
                 { typeof(DateTime[]), NpgsqlDbType.Timestamp | NpgsqlDbType.Array }
            };
        #endregion

        /// <summary>
        ///  重写父级 ExpressionVisitor.VisitBinary 方法
        /// </summary>
        /// <param name="node">运算表达式</param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            this.Left = node.Left;
            this.Right = node.Right;
            SqlText.LastNodeType = node.NodeType;
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                var result = VisitArrayIndex((BinaryExpression)node);
                var ptext = ParameterGenerated(out NpgsqlParameter p, result, node.NodeType);
                SqlText.Builder.Append(ptext);
            }
            else
            {
                SqlText.Builder.Append("(");
                Visit(node.Left);
                if (!OPERATOR_SET.TryGetValue(node.NodeType, out var operand))
                {
                    SqlText.Builder.AppendLine(node.ToString());
                }
                else SqlText.Builder.Append(operand);
                Visit(node.Right);
                SqlText.Builder.Append(")");
            }
            return node;
        }

        /// <summary>
        ///  根据表达式提取数组类型参数的值
        /// </summary>
        /// <param name="node">运算表达式</param>
        /// <returns></returns>
        private object VisitArrayIndex(BinaryExpression node)
        {
            var arrayAccessExpr = Expression.ArrayAccess(node.Left, node.Right);
            var fn = Expression.Lambda(arrayAccessExpr);
            var result = fn.Compile().DynamicInvoke();

            return result;
        }

        /// <summary>
        ///  重写父级 ExpressionVisitor.VisitUnary 方法
        /// </summary>
        /// <param name="node">运算表达式</param>
        /// <returns></returns>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not && node.Operand is MemberExpression)
            {
                this.SqlText.Builder.Append(OPERATOR_SET[ExpressionType.NotEqual]);
                this.SqlText.Builder.Append("true");
            }
            else if (node.NodeType == ExpressionType.Not && node.Operand is BinaryExpression) this.SqlText.Builder.Append(OPERATOR_SET[node.NodeType]);
            else if (node.NodeType == ExpressionType.Convert) Visit(node.Operand);
            else if (node.NodeType == ExpressionType.ArrayIndex) Visit(node.Operand);

            return node;
        }

        /// <summary>
        ///  重写父级 ExpressionVisitor.VisitMethodCall 方法
        /// </summary>
        /// <param name="node">运算表达式</param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                case "NotIn":
                case "In":
                    if (node.Object == null)
                    {
                        Visit(node.Arguments[0]);
                        ParamenterIn(node, node.Method.Name == "In");
                    }
                    break;
                case "NotLike":
                case "Like":
                    if (node.Object == null)
                    {
                        Visit(node.Arguments[0]);
                        ParamenterLike(node, node.Method.Name == "Like");
                    }
                    break;
                default:
                    var value = Expression.Lambda(node).Compile().DynamicInvoke();
                    var ptext = ParameterGenerated(out NpgsqlParameter p, value, node.NodeType);
                    this.SqlText.Builder.Append(ptext);
                    break;
            }
            return node;
        }

        /// <summary>
        ///  重写父级 ExpressionVisitor.VisitConstant 方法
        /// </summary>
        /// <param name="node">运算表达式</param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (this.Left is UnaryExpression)
            {
                var value = node.Value;
                Type type = ((UnaryExpression)this.Left).Operand.Type;
                if (type.GenericTypeArguments.Length > 0)
                    type = type.GenericTypeArguments[0];
                if (type.BaseType.Name == "Enum")
                {
                    value = Enum.Parse(type, node.Value.ToString());
                }
                Evaluate(type, value, node.NodeType);
            }
            else
                Evaluate(node.Type, node.Value, node.NodeType);
            return node;
        }

        /// <summary>
        ///  重写父级 ExpressionVisitor.VisitMember 方法
        /// </summary>
        /// <param name="node">运算表达式</param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            var noExper = node.Expression == null || node.Expression.NodeType != ExpressionType.Parameter;
            if (node.Member is FieldInfo fieldInfo && fieldInfo.IsStatic && fieldInfo.IsInitOnly) return base.VisitMember(node);
            else if (noExper && node.Member is PropertyInfo propertyInfo && !propertyInfo.PropertyType.IsGenericTypeDefinition) AccessProperty(node, propertyInfo);
            else if (noExper && node.Member is FieldInfo fi_member && !fi_member.FieldType.IsGenericTypeDefinition) AccessField(node, fi_member);
            else
            {
                if (node.Expression == null) this.SqlText.Builder.Append(node.Member.DeclaringType.Name);
                else
                {
                    if (node.Expression.NodeType == ExpressionType.Convert)
                    {
                        this.SqlText.Builder.Append("(");
                        Visit(node.Expression);
                        this.SqlText.Builder.Append(")");
                    }
                    else if (node.Expression.NodeType == ExpressionType.Constant)
                    {
                        if (node.Member is FieldInfo fi && fi.FieldType.IsArray && PG_TYPES.ContainsKey(fi.FieldType)) AccessField(node, fi);
                        else if (node.Member is PropertyInfo pi && pi.PropertyType.IsArray) AccessProperty(node, pi);
                    }
                    else Visit(node.Expression);
                }

                if (node.Expression is ParameterExpression)
                {
                    string fieldName = AliasGenerated(node.Member.DeclaringType, node.Member.Name);
                    this.SqlText.Builder.Append(fieldName);
                }
            }
            return node;
        }

        /// <summary>
        ///  重写父级 ExpressionVisitor.VisitNew 方法
        /// </summary>
        /// <param name="node">运算表达式</param>
        /// <returns></returns>
        protected override Expression VisitNew(NewExpression node)
        {
            var value = Expression.Lambda<Func<object>>(Expression.Convert(node, typeof(object))).Compile().Invoke();
            Evaluate(node.Type, value, node.NodeType);
            return node;
        }

        /// <summary>
        ///  根据运算表达式提取属性值
        /// </summary>
        /// <param name="node">运算表达式</param>
        /// <param name="propertyInfo">属性对象</param>
        /// <returns></returns>
        protected void AccessProperty(MemberExpression node, PropertyInfo propertyInfo)
        {
            var obj = Expression.Lambda<Func<object>>(Expression.Convert(node.Expression, typeof(object))).Compile().Invoke();
            var value = propertyInfo.GetValue(obj);
            Evaluate(propertyInfo.PropertyType, value, node.NodeType);
        }

        /// <summary>
        ///  根据运算表达式提取字段值
        /// </summary>
        /// <param name="node">运算表达式</param>
        /// <param name="fieldInfo">字段对象</param>
        /// <returns></returns>
        protected void AccessField(MemberExpression node, FieldInfo fieldInfo)
        {
            var obj = Expression.Lambda<Func<object>>(Expression.Convert(node.Expression, typeof(object))).Compile().Invoke();
            var value = fieldInfo.GetValue(obj);
            Evaluate(fieldInfo.FieldType, value, node.NodeType);
        }

        /// <summary>
        ///  根据传入的对象值封装数据库查询参数
        /// </summary>
        /// <param name="type">值类型</param>
        /// <param name="value">值</param>
        /// <param name="nodeType">运算表达式节点类型</param>
        protected void Evaluate(Type type, object value, ExpressionType nodeType)
        {
            Type specificType = null;
            NpgsqlDbType? dbType = null;
            if (type.IsEnum)
            {
                specificType = type;
                dbType = NpgsqlDbType.Enum;
            }
            if (type.IsArray)
            {
                if (PG_TYPES.ContainsKey(type)) dbType = PG_TYPES[type];
                else
                {
                    var ft = type.Assembly.GetType(type.FullName.Replace("[]", ""));
                    if (ft.IsEnum)
                    {
                        specificType = ft;
                        dbType = NpgsqlDbType.Enum | NpgsqlDbType.Array;
                    }
                }
            }
            var text = ParameterGenerated(out NpgsqlParameter p, value, nodeType, dbType);
            if (p != null) p.SpecificType = specificType;

            this.SqlText.Builder.Append(text);
        }

        /// <summary>
        ///  根据传入的类型查找数据库表指定的别名
        /// </summary>
        /// <param name="type">数据库表对应的实体对象类型</param>
        /// <param name="memberName">字段名称</param>
        /// <returns></returns>
        protected string AliasGenerated(Type type, string memberName)
        {
            string tableName = type == TypeMaster ? AliasMaster : AliasUnion;
            return tableName == null ? memberName : (tableName + "." + memberName);
        }

        /// <summary>
        ///  根据运算表达式提取数据库查询函数 in/not in 的参数
        /// </summary>
        /// <param name="node">运算表达式</param>
        /// <param name="isIn">当前操作是 in（true）还是 not in (false) </param>
        protected void ParamenterIn(MethodCallExpression node, bool isIn)
        {
            var f = Expression.Lambda(node.Arguments[1]).Compile();
            ICollection _value = (ICollection)f.DynamicInvoke();
            List<string> keys = new List<string>();
            IEnumerator rator = _value.GetEnumerator();
            while (rator.MoveNext())
            {
                string p_key = Guid.NewGuid().ToString("N");
                NpgsqlParameter parameter = new NpgsqlParameter(p_key, rator.Current);
                this.SqlText.Parameters.Add(parameter);
                keys.Add("@" + p_key);
            }

            string methodName = isIn ? " IN " : " NOT IN ";
            if (keys.Count == 0) throw new ArgumentNullException($"{methodName} 查询必须提供参数，{node}");
            var text = $" {methodName.ToUpper()} ({string.Join(",", keys)})";
            this.SqlText.Builder.Append(text);
        }

        /// <summary>
        ///  根据运算表达式提取数据库查询函数 like/not like 的参数
        /// </summary>
        /// <param name="node">运算表达式</param>
        /// <param name="isIn">当前操作是 like（true）还是 not like (false) </param>
        protected void ParamenterLike(MethodCallExpression node, bool isLike)
        {
            string methodName = isLike ? "ILIKE " : " NOT ILIKE ";
            this.SqlText.Builder.Append($" {methodName} '%' || ");
            Visit(node.Arguments[1]);
            this.SqlText.Builder.Append(" || '%'");
        }

        /// <summary>
        ///  根据传入的值封装一个数据库查询参数
        /// </summary>
        /// <param name="parameter">输出参数对象</param>
        /// <param name="value">待封装到参数的值</param>
        /// <param name="nodeType">当前运算表达式的操作符对象</param>
        /// <param name="dbType">指定的数据库类型，可为 null</param>
        /// <returns></returns>
        protected string ParameterGenerated(out NpgsqlParameter parameter, object value, ExpressionType nodeType, NpgsqlDbType? dbType = null)
        {
            parameter = null;
            string pname = string.Empty;
            if (value == null)
            {
                this.SqlText.Builder.Remove(this.SqlText.Builder.Length - 3, 3);
                if (this.SqlText.LastNodeType == ExpressionType.Equal) pname = " IS NULL";
                else if (this.SqlText.LastNodeType == ExpressionType.NotEqual) pname = " IS NOT NULL";
            }
            else
            {
                string p_key = Guid.NewGuid().ToString("N");
                parameter = new NpgsqlParameter(p_key, value);
                if (dbType.HasValue)
                {
                    parameter.NpgsqlDbType = dbType.Value;
                }
                this.SqlText.Parameters.Add(parameter);
                pname = $"@{p_key}";
            }

            return pname;
        }
    }

    /// <summary>
    ///  数据库查询包装实体对象
    /// </summary>
    public class SqlTextEntity
    {
        /// <summary>
        ///  获取或者设置 Sql 命令封装对象
        /// </summary>
        public StringBuilder Builder { get; set; } = new StringBuilder();
        /// <summary>
        ///  获取或者设置解析后得到的数据库查询参数对象
        /// </summary>
        public List<NpgsqlParameter> Parameters { get; set; } = new List<NpgsqlParameter>();
        /// <summary>
        ///  当前表达式最后一个运算操作符
        /// </summary>
        public ExpressionType LastNodeType { get; set; }
    }
}