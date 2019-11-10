using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;

using Dapper;


namespace devhl.DBAutomator
{
    internal static class PostgresMethods
    {
        public static bool IsStorable(this PropertyInfo propertyInfo)
        {
            //if (typeof(INotMapped).IsAssignableFrom(propertyInfo.GetType()))
            //{
            //    return false;
            //}

            if (propertyInfo.GetCustomAttributes<NotMappedAttribute>(true).FirstOrDefault() is NotMappedAttribute notMappedAttribute)
            {
                return false;
            }

            if (propertyInfo.PropertyType == typeof(string) ||
                propertyInfo.PropertyType == typeof(ulong) ||
                propertyInfo.PropertyType == typeof(ulong?) ||
                propertyInfo.PropertyType == typeof(bool) ||
                propertyInfo.PropertyType == typeof(bool?) ||
                propertyInfo.PropertyType == typeof(decimal) ||
                propertyInfo.PropertyType == typeof(decimal?) ||
                propertyInfo.PropertyType == typeof(DateTime) ||
                propertyInfo.PropertyType == typeof(DateTime?) ||
                propertyInfo.PropertyType == typeof(int) ||
                propertyInfo.PropertyType == typeof(int?) ||
                propertyInfo.PropertyType == typeof(long) ||
                propertyInfo.PropertyType == typeof(long?) ||

                propertyInfo.PropertyType.IsEnum
                )
            {
                return true;
            }
            return false;
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

        public static string GetWhereClause<C>(this Expression<Func<C, object>>? e, List<ExpressionModel<C>> expressions)
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

        public static string GetWhereClause<C>(C item, List<RegisteredProperty> registeredProperties, string delimiter = "AND")
        {
            if (item == null)
            {
                throw new NullReferenceException("The item cannot be null.");
            }

            string result = string.Empty;

            foreach (var property in registeredProperties)
            {
                result = $"{result} \"{property.ColumnName}\" = @{property.ColumnName} {delimiter}";
            }

            int remove = delimiter.Length + 1;

            result = result[0..^remove];

            return result;
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

        public static DynamicParameters GetDynamicParameters<C>(C item, List<RegisteredProperty> registeredProperties)
        {
            if (item == null)
            {
                throw new NullReferenceException("The item cannot be null.");
            }

            DynamicParameters p = new DynamicParameters();

            foreach (var property in registeredProperties.Where(p => !p.IsAutoIncrement))
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

        private static string GetMathSymbol(this ExpressionType expression) =>
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










    }
}
