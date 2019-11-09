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

        public static string GetWhereClause<C>(this Expression<Func<C, object>>? e, RegisteredClass registeredClass)
        {
            if (e == null)
            {
                return string.Empty;
            }

            if (e.Body is UnaryExpression unaryExpression && unaryExpression.Operand is BinaryExpression binaryExpression)
            {

                string result = $"{binaryExpression.Left.ToString()} {binaryExpression.NodeType} {binaryExpression.Right.ToString()}";

                result = result
                    .Replace("GreaterThanOrEqual", ">=")
                    .Replace("LessThanOrEqual", "<=")
                    .Replace("AndAlso", "AND")
                    .Replace("OrElse", "OR")
                    .Replace("\"", "'")
                    .Replace("GreaterThan", ">")
                    .Replace("LessThan", "<")
                    .Replace("Equal", "=");
                ;

                List<string> parameterNames = new List<string>();

                List<string> columnNames = new List<string>();

                GetParameterNames(binaryExpression, parameterNames, columnNames);

                foreach(var str in parameterNames)
                {
                    result = result.Replace($"{str}.", "");
                }

                foreach(var str in columnNames)
                {
                    var columnName = registeredClass.RegisteredProperties.First(p => p.PropertyName == str).ColumnName;

                    result = result.Replace(str, $"\"{columnName}\"");
                }

                result = result.Replace("==", "=");

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
            
            foreach(var property in registeredClass.RegisteredProperties)
            {
                result = $"{result} \"{property.ColumnName}\" =";

                if (property.PropertyType == typeof(string))
                {
                    result = $"{result} '{item.GetType().GetProperty(property.PropertyName).GetValue(item, null)}' {delimiter}";
                }
                else
                {
                    result = $"{result} {item.GetType().GetProperty(property.PropertyName).GetValue(item)} {delimiter}";
                }
            }

            int remove = delimiter.Length + 1;

            result = result[0..^remove];

            return result;
        }

        private static void GetParameterNames(BinaryExpression binaryExpression, List<string> parameterNames, List<string> columnNames)
        {
            if (binaryExpression.Left is BinaryExpression left)
            {
                GetParameterNames(left, parameterNames, columnNames);
            }

            if (binaryExpression.Right is BinaryExpression right)
            {
                GetParameterNames(right, parameterNames, columnNames);
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
        }
    }
}
