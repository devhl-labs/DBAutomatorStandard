using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DBAutomatorStandard
{
    internal static class Statics
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();

        public static string Left(this string param, int length)
        {
            string result = param.Substring(0, length);
            return result;
        }

        public static string Right(this string param, int length)
        {
            string result = param.Substring(param.Length - length, length);
            return result;
        }

        public static BinaryExpression? GetBinaryExpression<C>(Expression<Func<C, object>>? where = null)
        {
            UnaryExpression? unaryExpression;

            BinaryExpression? binaryExpression = null;

            if (where != null)
            {
                unaryExpression = (UnaryExpression)where.Body;

                binaryExpression = (BinaryExpression)unaryExpression.Operand;
            }

            return binaryExpression;
        }

        public static string GetMathSymbol(this ExpressionType expression) =>
            expression switch
            {
                ExpressionType.Equal => "==",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.OrElse => "OR",
                ExpressionType.AndAlso => "AND",

                _ => throw new ArgumentException("Invalid enum value", nameof(expression))
            };
        
        public static string GetWhereClause<C>(this Expression<Func<C, object>>? e, RegisteredClass registeredClass, List<ExpressionModel<C>> expressions)
        {
            if (e == null)
            {
                return string.Empty;
            }

            if (e.Body is UnaryExpression unaryExpression && unaryExpression.Operand is BinaryExpression binaryExpression)
            {
                string result = $"{binaryExpression.Left.ToString()} {binaryExpression.NodeType.GetMathSymbol()} {binaryExpression.Right.ToString()}";

                if (expressions != null)
                {
                    foreach (var exp in expressions)
                    {
                        result = result.Replace(exp.ToString(), exp.ToSqlString());
                    }
                }

                result = result.Replace(") OrElse (", ") OR (").Replace(") AndAlso (", ") AND (");

                return result;
            }

            return string.Empty;
        }

        public static string GetWhereClause<C>(C item, RegisteredClass registeredClass, string delimiter = "AND")
        {
            if (item == null)
            {
                throw new NullReferenceException("The item cannot be null.");
            }

            string result = string.Empty;

            foreach (var property in registeredClass.RegisteredProperties)
            {
                result = $"{result} \"{property.ColumnName}\" = @{property.ColumnName} {delimiter}";
            }

            int remove = delimiter.Length + 1;

            result = result[0..^remove];

            return result;
        }

        public static DynamicParameters GetDynamicParameters<C>(C item, RegisteredClass registeredClass)
        {
            if (item == null)
            {
                throw new NullReferenceException("The item cannot be null.");
            }

            DynamicParameters p = new DynamicParameters();

            foreach (var property in registeredClass.RegisteredProperties)
            {
                if (property.PropertyType == typeof(string))
                {
                    p.Add($"@{property.ColumnName}", item.GetType().GetProperty(property.PropertyName).GetValue(item, null));
                }
                else if (property.PropertyType == typeof(ulong))
                {
                    p.Add($"@{property.ColumnName}", Convert.ToInt64(item.GetType().GetProperty(property.PropertyName).GetValue(item, null)));
                }
                else
                {
                    p.Add($"@{property.ColumnName}", item.GetType().GetProperty(property.PropertyName).GetValue(item));
                }
            }

            return p;
        }

        public static void GetExpressions<C>(BinaryExpression? binaryExpression, List<ExpressionModel<C>> expressions, RegisteredClass registeredClass, string parameterPrefix = "w_")
        {
            if (binaryExpression == null)
            {
                return;
            }

            if (binaryExpression.Left is BinaryExpression left)
            {
                GetExpressions(left, expressions, registeredClass);
            }

            if (binaryExpression.Right is BinaryExpression right)
            {
                GetExpressions(right, expressions, registeredClass);
            }

            ExpressionModel<C> expression = new ExpressionModel<C>(registeredClass, parameterPrefix)
            {
                NodeType = binaryExpression.NodeType
            };

            if (binaryExpression.Left is MemberExpression memberExpression)
            {
                if (memberExpression.Expression is ParameterExpression thisExpression)
                {
                    expression.ParameterName = thisExpression.Name;
                }

                expression.PropertyName = memberExpression.Member.Name;
            }

            if (binaryExpression.Right is ConstantExpression constantExpression)
            {
                expression.Value = constantExpression.Value;
            }

            if (expression.PropertyName != null)
            {
                expressions.Add(expression);
            }
        }

        private static void GetParameterNames(BinaryExpression binaryExpression, List<string> parameterNames, List<string> columnNames, List<object> values)
        {
            if (binaryExpression.Left is BinaryExpression left)
            {
                GetParameterNames(left, parameterNames, columnNames, values);
            }

            if (binaryExpression.Right is BinaryExpression right)
            {
                GetParameterNames(right, parameterNames, columnNames, values);
            }

            if (binaryExpression.Left is MemberExpression memberExpression)
            {
                if (memberExpression.Expression is ParameterExpression thisExpression)
                {
                    if (!parameterNames.Any(p => p == thisExpression.Name))
                    {
                        parameterNames.Add(thisExpression.Name);
                    }
                }

                if (!columnNames.Any(c => c == memberExpression.Member.Name))
                {
                    columnNames.Add(memberExpression.Member.Name);
                }
            }

            if (binaryExpression.Right is ConstantExpression constantExpression)
            {
                values.Add(constantExpression.Value);
            }
        }

        public static DynamicParameters GetDynamicParametersFromExpression<C>(List<ExpressionModel<C>> expressions, DynamicParameters? p = null)
        {
            p ??= new DynamicParameters();

            foreach (var exp in expressions)
            {
                if (exp.Value is ulong)
                {
                    p.Add($"{exp.ParameterPrefix}{exp.ColumnName}", Convert.ToInt64(exp.Value));
                }
                else
                {
                    p.Add($"{exp.ParameterPrefix}{exp.ColumnName}", exp.Value);
                }
            }

            return p;
        }
        
    }
}
