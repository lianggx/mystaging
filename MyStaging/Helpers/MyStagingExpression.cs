using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  lambda 表达式解析对象
    /// </summary>
    public class PgSqlExpression
    {
        /// <summary>
        ///  获取或者左侧表达式
        /// </summary>
        public Expression Left { get; set; }

        /// <summary>
        ///  获取或者设置右侧表达式
        /// </summary>
        public Expression Right { get; set; }

        /// <summary>
        /// 获取或者设置当前表达式的主对象类型，用于生成数据库关系对象名称
        /// </summary>
        public Type MasterType { get; set; }

        /// <summary>
        /// 获取或者设置当前表达式的主对象类型的别名
        /// </summary>
        public string Master_AlisName { get; set; }

        /// <summary>
        ///  获取或者设置当前表达式的连接查询对象类型
        /// </summary>
        public string Union_AlisName { get; set; }

        /// <summary>
        ///  获取或者设置输出的 SQL 语句
        /// </summary>
        public StringBuilder CommandText { get; set; } = new StringBuilder();

        /// <summary>
        ///  获取或者设置当前表达式生成的数据库参数列表
        /// </summary>
        public List<NpgsqlParameter> Parameters { get; set; } = new List<NpgsqlParameter>();

        /// <summary>
        ///  解析表达式提供程序
        /// </summary>
        /// <param name="left">左侧表达式</param>
        /// <param name="right">右侧表达式</param>
        /// <param name="type">表达式节点类型</param>
        protected void ExpressionProvider(Expression left, Expression right, ExpressionType type)
        {
            this.Left = left;
            this.Right = right;
            CommandText.Append("(");
            // left
            ExpressionCapture(left, type);
            CommandText.Append(" ");
            CommandText.Append(NodeTypeToString(type));
            CommandText.Append(" ");
            // right
            ExpressionCapture(right, type);
            CommandText.Append(")");
        }

        /// <summary>
        ///  表达式解析主入口方法
        /// </summary>
        /// <param name="selector"></param>
        public void ExpressionCapture(Expression selector)
        {
            ExpressionCapture(selector, selector.NodeType);
        }

        /// <summary>
        ///  表达式解析主入口方法，方法重载
        /// </summary>
        /// <param name="selector">待解析的表达式</param>
        /// <param name="parent_type">父级表达式节点类型</param>
        private void ExpressionCapture(Expression selector, ExpressionType parent_type)
        {
            if (selector is BinaryExpression be)
            {
                ExpressionProvider(be.Left, be.Right, be.NodeType);
            }
            else if (selector is MemberExpression me)
            {
                if (!selector.ToString().StartsWith("value(")
                    && selector.Type != typeof(DateTime)
                    ||
                    (
                    me.Expression != null
                    && me.Expression.NodeType == ExpressionType.Parameter
                    && !me.ToString().StartsWith("value(")
                    && me.Type == typeof(DateTime)
                    ))
                {
                    string tableName = me.Member.DeclaringType == MasterType ? Master_AlisName : Union_AlisName;
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        tableName = tableName + ".";
                    }
                    CommandText.Append($"{tableName}{me.Member.Name}");
                }
                else
                {
                    InvokeExpression(selector, parent_type);
                }
            }
            else if (selector is NewArrayExpression ae)
            {
                foreach (Expression ex in ae.Expressions)
                {
                    ExpressionCapture(ex, parent_type);
                    CommandText.Append(",");
                }
                CommandText.Remove(CommandText.Length - 1, 1);
            }
            else if (selector is MethodCallExpression callExp)
            {
                CommandText.Append("(");
                switch (callExp.Method.Name)
                {
                    case "Like":
                        ExpressionCapture(callExp.Arguments[0], parent_type);
                        CommandText.Append($" ILIKE '%' || ");
                        ExpressionCapture(callExp.Arguments[1], parent_type);
                        CommandText.Append(" || '%'");
                        break;
                    case "NotLike":
                        ExpressionCapture(callExp.Arguments[0], parent_type);
                        CommandText.Append($" NOT ILIKE '%' || ");
                        ExpressionCapture(callExp.Arguments[1], parent_type);
                        CommandText.Append(" || '%'");
                        break;
                    case "In":
                        ExpressionCapture(callExp.Arguments[0], parent_type);
                        In_Not_Parameter(callExp.Arguments[1], "IN");
                        break;
                    case "NotIn":
                        ExpressionCapture(callExp.Arguments[0], parent_type);
                        In_Not_Parameter(callExp.Arguments[1], "NOT IN");
                        break;
                    default:
                        try
                        {
                            InvokeExpression(selector, parent_type);
                        }
                        catch (Exception ex)
                        {
                            throw new NotSupportedException("使用数据库字段和CLR对象进行函数计算查询时，仅支持 Like,NotLike,In,NotIn 查询", ex);
                        }
                        break;
                }
                CommandText.Append(")");
            }
            else if (selector is ConstantExpression ce)
            {
                if (this.Left is UnaryExpression)
                {
                    Type type = ((UnaryExpression)this.Left).Operand.Type;
                    if (type.BaseType.Name == "Enum")
                    {
                        object val = Enum.Parse(type, ce.Value.ToString());
                        SetValue(val, parent_type, type);
                    }
                    else
                        SetValue(ce.Value, parent_type);
                }
                else
                    SetValue(ce.Value, parent_type);
            }
            else if (selector is UnaryExpression ue)
            {
                ExpressionCapture(ue.Operand, parent_type);
            }
            else if (selector is ParameterExpression)
            {
                InvokeExpression(selector, parent_type);
            }
            else if (selector is NewExpression)
            {
                InvokeExpression(selector, parent_type);
            }
        }

        /// <summary>
        ///  将表达式转换为数据库的 in/not in 数组操作
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="method"></param>
        protected void In_Not_Parameter(Expression exp, string method)
        {
            var f = Expression.Lambda(exp).Compile();
            ICollection _value = (ICollection)f.DynamicInvoke();
            List<string> keys = new List<string>();
            IEnumerator rator = _value.GetEnumerator();
            while (rator.MoveNext())
            {
                string p_key = Guid.NewGuid().ToString("N");
                NpgsqlParameter parameter = new NpgsqlParameter(p_key, rator.Current);
                Parameters.Add(parameter);
                keys.Add("@" + p_key);
            }
            if (keys.Count == 0)
                throw new ArgumentNullException($"{method} 查询必须提供参数，{exp}");
            CommandText.Append($" {method} ({string.Join(",", keys)})");
        }

        /// <summary>
        ///  动态执行表达式，获得执行后的值
        /// </summary>
        /// <param name="exp">待执行的表达式</param>
        /// <param name="parent_type">父级表达式节点类型</param>
        protected void InvokeExpression(Expression exp, ExpressionType parent_type)
        {
            var f = Expression.Lambda(exp).Compile();
            object _value = f.DynamicInvoke();
            SetValue(_value, parent_type);
        }

        /// <summary>
        /// 根据传入的值，创建一个 NpgsqlParameter 对象，并装入属性 Parameters 中
        /// </summary>
        /// <param name="val">参数的值</param>
        /// <param name="type">运算符类型</param>
        /// <param name="specificType">数据库指定列的类型</param>
        protected void SetValue(object val, ExpressionType type, Type specificType = null)
        {
            if (val == null)
            {
                CommandText.Remove(CommandText.Length - 3, 3);
                if (type == ExpressionType.Equal)
                    CommandText.Append(" IS NULL");
                else if (type == ExpressionType.NotEqual)
                    CommandText.Append(" IS NOT NULL");
            }
            else
            {
                string p_key = Guid.NewGuid().ToString("N");
                NpgsqlParameter parameter = null;
                if (specificType != null)
                {
                    parameter = new NpgsqlParameter(p_key, NpgsqlDbType.Enum);
                    parameter.SpecificType = specificType;
                    parameter.Value = val;
                }
                else
                    parameter = new NpgsqlParameter(p_key, val);
                Parameters.Add(parameter);
                CommandText.Append($"@{p_key}");
            }
        }

        /// <summary>
        ///  根据传入的节点类型，返回对应的数据库运算符
        /// </summary>
        /// <param name="type">运算符类型</param>
        /// <returns></returns>
        protected string NodeTypeToString(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                default:
                    return null;
            }
        }
    }
}
