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
    public class PgSqlExpression
    {
        public Expression Left { get; set; }
        public Expression Right { get; set; }
        public Type MasterType { get; set; }
        public string Master_AlisName { get; set; }
        public string Union_AlisName { get; set; }
        public StringBuilder CommandText { get; set; } = new StringBuilder();
        public List<NpgsqlParameter> Parameters { get; set; } = new List<NpgsqlParameter>();
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

        public void ExpressionCapture(Expression selector)
        {
            ExpressionCapture(selector, selector.NodeType);
        }

        private void ExpressionCapture(Expression selector, ExpressionType parent_type)
        {
            if (selector is BinaryExpression)
            {
                BinaryExpression be = ((BinaryExpression)selector);
                ExpressionProvider(be.Left, be.Right, be.NodeType);
            }
            else if (selector is MemberExpression)
            {
                if (!selector.ToString().StartsWith("value("))
                {
                    MemberExpression me = ((MemberExpression)selector);
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
            else if (selector is NewArrayExpression)
            {
                NewArrayExpression ae = ((NewArrayExpression)selector);
                foreach (Expression ex in ae.Expressions)
                {
                    ExpressionCapture(ex, parent_type);
                    CommandText.Append(",");
                }
                CommandText.Remove(CommandText.Length - 1, 1);
            }
            else if (selector is MethodCallExpression)
            {
                MethodCallExpression callExp = (MethodCallExpression)selector;
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
            else if (selector is ConstantExpression)
            {
                ConstantExpression ce = ((ConstantExpression)selector);
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
            else if (selector is UnaryExpression)
            {
                UnaryExpression ue = ((UnaryExpression)selector);
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

        protected void InvokeExpression(Expression exp, ExpressionType parent_type)
        {
            var f = Expression.Lambda(exp).Compile();
            object _value = f.DynamicInvoke();
            SetValue(_value, parent_type);
        }

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
