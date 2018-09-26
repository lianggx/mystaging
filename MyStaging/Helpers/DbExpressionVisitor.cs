using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MyStaging.Helpers
{
    public class ExpressionTrasfer : ExpressionVisitor
    {
        private static Dictionary<ExpressionType, string> operatorSet = new Dictionary<ExpressionType, string>()
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

        public string GenerateOperator(ExpressionType op) => operatorSet[op];

        public StringBuilder CommandTextBuilder { get; set; } = new StringBuilder();

        public List<NpgsqlParameter> Parameters { get; set; } = new List<NpgsqlParameter>();

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                var result = VisitArrayIndex((BinaryExpression)node);
                var ptext = CreateParameter(out NpgsqlParameter p, result, node.NodeType);
                CommandTextBuilder.Append(ptext);
            }
            else
            {
                CommandTextBuilder.Append("(");
                Visit(node.Left);
                if (!operatorSet.TryGetValue(node.NodeType, out var operand))
                {
                    CommandTextBuilder.AppendLine(node.ToString());
                }
                else
                {
                    CommandTextBuilder.Append(operand);
                }

                Visit(node.Right);
                CommandTextBuilder.Append(")");
            }
            return node;
        }

        private object VisitArrayIndex(BinaryExpression node)
        {
            var arrayAccessExpr = Expression.ArrayAccess(node.Left, node.Right);
            var fn = Expression.Lambda(arrayAccessExpr);
            var result = fn.Compile().DynamicInvoke();

            return result;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not && node.Operand is MemberExpression)
            {
                CommandTextBuilder.Append(GenerateOperator(ExpressionType.NotEqual));
                CommandTextBuilder.Append("true");
            }
            else if (node.NodeType == ExpressionType.Not && node.Operand is BinaryExpression)
            {
                CommandTextBuilder.Append(GenerateOperator(node.NodeType));
            }
            else if (node.NodeType == ExpressionType.Convert)
            {
                Visit(node.Operand);
            }
            else if (node.NodeType == ExpressionType.ArrayIndex)
                Visit(node.Operand);

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                case "Contains":
                    if (node.Object == null)
                    {
                        string text = In(node, true);
                        CommandTextBuilder.Append(text);
                    }
                    break;
                case "Parse":
                    var val = Expression.Lambda(node).Compile().DynamicInvoke();
                    var ptext = CreateParameter(out NpgsqlParameter p, val, node.NodeType);
                    CommandTextBuilder.Append(ptext);
                    break;
                default:
                    base.VisitMethodCall(node);
                    break;
            }
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            NpgsqlParameter p = null;
            string ptext = CreateParameter(out p, node.Value, node.NodeType);
            CommandTextBuilder.Append(ptext);
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member is FieldInfo fieldInfo && fieldInfo.IsStatic && fieldInfo.IsInitOnly)
                return base.VisitMember(node);
            else if ((node.Expression == null || node.Expression.NodeType != ExpressionType.Parameter)
                     && node.Member is PropertyInfo propertyInfo
                     && !propertyInfo.PropertyType.IsGenericType)
            {
                var obj = Expression.Lambda(node).Compile().DynamicInvoke();
                var text = CreateParameter(out NpgsqlParameter p, obj, node.NodeType);
                CommandTextBuilder.Append(text);
            }
            else
            {
                if (node.Expression != null)
                {
                    if (node.Expression.NodeType == ExpressionType.Convert)
                    {
                        CommandTextBuilder.Append("(");
                        Visit(node.Expression);
                        CommandTextBuilder.Append(")");
                    }
                    else if (node.Expression.NodeType == ExpressionType.Constant)
                    {
                        if (node.Member is FieldInfo fi && fi.FieldType.IsArray)
                        {
                            var obj = Expression.Lambda(node).Compile().DynamicInvoke();
                            NpgsqlDbType dbtype = FindArrayDbType(fi.FieldType.FullName);
                            var text = CreateParameter(out NpgsqlParameter p, obj, node.NodeType, dbtype);

                            CommandTextBuilder.Append(text);
                        }
                        else if (node.Member is PropertyInfo pi && pi.PropertyType.IsArray)
                        {
                        }

                    }
                    else
                    {
                        Visit(node.Expression);
                    }
                }
                else
                {
                    CommandTextBuilder.Append(node.Member.DeclaringType.Name);
                }

                if (node.Expression is ParameterExpression)
                    CommandTextBuilder.Append(node.Member.Name);
            }
            return node;
        }

        private NpgsqlDbType FindArrayDbType(string typeName)
        {
            NpgsqlDbType dbType = NpgsqlDbType.Array;

            typeName = typeName.Remove(typeName.Length - 2, 2);
            switch (typeName)
            {
                case "System.String":
                    dbType = dbType | NpgsqlDbType.Varchar;
                    break;
            }

            return dbType;
        }

        private string GetAlisName(Type type, string memberName)
        {
            return null;
        }

        public string In(MethodCallExpression expression, object isTrue)
        {

            var arg1 = (expression.Arguments[0] as MemberExpression).Expression as ConstantExpression;
            var arg2 = expression.Arguments[1] as MemberExpression;
            var Field_Array = arg1.Value.GetType().GetFields().Last();
            object[] Array = Field_Array.GetValue(arg1.Value) as object[];
            List<string> inPara = new List<string>();
            for (int i = 0; i < Array.Length; i++)
            {
                string key = "@" + i;
                string value = Array[i].ToString();

                NpgsqlParameter p = null;
                string pname = CreateParameter(out p, value, expression.NodeType);
                inPara.Add(pname);
            }
            string memberName = GetAlisName(arg2.Member.DeclaringType, arg2.Member.Name);
            string oper = Convert.ToBoolean(isTrue) ? " IN " : " NOT IN ";
            string compname = string.Join(",", inPara);
            string result = string.Format("{0} {1} ({2})", memberName, oper, compname);

            return result;
        }

        public string CreateParameter(out NpgsqlParameter parameter, object val, ExpressionType type, NpgsqlDbType? dbType = null)
        {
            parameter = null;
            string pname = string.Empty;
            if (val == null)
            {
                val = Convert.ChangeType(val, val.GetType());
                CommandTextBuilder.Remove(CommandTextBuilder.Length - 3, 3);
                if (type == ExpressionType.Equal)
                    pname = " IS NULL";
                else if (type == ExpressionType.NotEqual)
                    pname = " IS NOT NULL";
            }
            else
            {
                string p_key = Guid.NewGuid().ToString("N");
                if (dbType.HasValue)
                {
                    parameter = new NpgsqlParameter(p_key, dbType);
                    parameter.Value = val;
                }
                else
                {
                    parameter = new NpgsqlParameter(p_key, val);
                }
                Parameters.Add(parameter);
                pname = $"@{p_key}";
            }

            return pname;
        }
    }
}



