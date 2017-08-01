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
        public Type MainType { get; set; }
        public string Main_AlisName { get; set; }
        public string Union_AlisName { get; set; }
        public StringBuilder CommandText { get; set; } = new StringBuilder();
        public List<NpgsqlParameter> Parameters { get; set; } = new List<NpgsqlParameter>();
        protected void ExpressionProvider(Expression left, Expression right, ExpressionType type)
        {
            CommandText.Append("(");
            // left
            MemberExpression me = left as MemberExpression;
            ExpressionCapture(left);
            CommandText.Append(" ");
            CommandText.Append(NodeTypeToString(type));
            CommandText.Append(" ");
            object _value = null;
            // right
            if (me != null && me.Expression is ParameterExpression)
            {
                try
                {
                    var f = Expression.Lambda(right).Compile();
                    _value = f.DynamicInvoke();
                    SetValue(_value, type);
                }
                catch
                {
                    ExpressionCapture(right);
                }
            }
            else
                ExpressionCapture(right);

            CommandText.Append(")");
        }

        protected void SetValue(object _value, ExpressionType type)
        {
            if (_value == null)
            {
                CommandText.Remove(CommandText.Length - 3, 3);
                if (type == ExpressionType.Equal)
                    CommandText.Append("IS NULL");
                else if (type == ExpressionType.NotEqual)
                    CommandText.Append("IS NOT NULL");
            }
            else
            {
                string p_key = Guid.NewGuid().ToString("N");
                NpgsqlParameter _p = new NpgsqlParameter(p_key, _value);
                Parameters.Add(_p);
                CommandText.Append($"@{p_key}");
            }
        }

        private string GetTableName(Type type)
        {
            string tableName;
            if (MainType != null)
                tableName = type == MainType ? Main_AlisName : Union_AlisName;
            else
                tableName = type.Name;

            return tableName;
        }

        public void ExpressionCapture(Expression selector)
        {
            if (selector is BinaryExpression)
            {
                BinaryExpression be = ((BinaryExpression)selector);
                ExpressionProvider(be.Left, be.Right, be.NodeType);
            }
            else if (selector is MemberExpression)
            {
                MemberExpression me = ((MemberExpression)selector);
                //string tableName = GetTableName(me.Member.DeclaringType);
                //if (tableName != null)
                //{
                //    tableName = $"{tableName}.";
                //}
                string tableName = me.Member.DeclaringType == MainType ? Main_AlisName : Union_AlisName;
                if (!string.IsNullOrEmpty(tableName))
                {
                    tableName = tableName + ".";
                }
                CommandText.Append($"{tableName}{me.Member.Name}");
            }
            else if (selector is NewArrayExpression)
            {
                NewArrayExpression ae = ((NewArrayExpression)selector);
                foreach (Expression ex in ae.Expressions)
                {
                    ExpressionCapture(ex);
                    CommandText.Append(",");
                }
                CommandText.Remove(CommandText.Length - 1, 1);
            }
            else if (selector is MethodCallExpression)
            {
                MethodCallExpression mce = (MethodCallExpression)selector;
                CommandText.Append("(");
                ExpressionCapture(mce.Arguments[0]);
                switch (mce.Method.Name)
                {
                    case "Like":
                        CommandText.Append($" LIKE '%");
                        ExpressionCapture(mce.Arguments[1]);
                        CommandText.Append("%'");
                        break;
                    case "NotLike":
                        CommandText.Append($" NOT LIKE '%");
                        ExpressionCapture(mce.Arguments[1]);
                        CommandText.Append("%'");
                        break;
                    case "In":
                        CommandText.Append($" IN (");
                        ExpressionCapture(mce.Arguments[1]);
                        CommandText.Append(")");
                        break;
                    case "NotIn":
                        CommandText.Append($" NOT IN (");
                        ExpressionCapture(mce.Arguments[1]);
                        CommandText.Append(")");
                        break;
                }
                CommandText.Append(")");
            }
            else if (selector is ConstantExpression)
            {
                ConstantExpression ce = ((ConstantExpression)selector);
                SetValue(ce.Value, ce.NodeType);
            }
            else if (selector is UnaryExpression)
            {
                UnaryExpression ue = ((UnaryExpression)selector);
                ExpressionCapture(ue.Operand);
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
