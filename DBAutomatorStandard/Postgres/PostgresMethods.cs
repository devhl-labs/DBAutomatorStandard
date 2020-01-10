using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;

using Dapper;

using MiaPlaza.ExpressionUtils;

using static devhl.DBAutomator.Statics;
using MiaPlaza.ExpressionUtils.Evaluating;
using devhl.DBAutomator.Models;

namespace devhl.DBAutomator
{
    internal static class PostgresMethods
    {
        private static void AddDynamicParameter<C>(this DynamicParameters p, string parameterName, C item, RegisteredProperty registeredProperty) //todo where C : notnull
        {
            if (item == null) throw new DbAutomatorException("The item cannot be null.", new ArgumentNullException("item"));

            if (registeredProperty.PropertyType == typeof(string))
            {
                p.Add(parameterName, item.GetType().GetProperty(registeredProperty.PropertyName).GetValue(item, null));
            }
            else if (registeredProperty.PropertyType == typeof(ulong))
            {
                p.Add(parameterName, Convert.ToInt64(item.GetType().GetProperty(registeredProperty.PropertyName).GetValue(item, null)));
            }
            else if (registeredProperty.PropertyType == typeof(ulong?))
            {
                ulong? id = (ulong?) item.GetType().GetProperty(registeredProperty.PropertyName).GetValue(item, null);

                if (id == null)
                {
                    p.Add(parameterName, null);
                }
                else
                {
                    p.Add(parameterName, Convert.ToInt64(id));
                }
            }
            else
            {
                p.Add(parameterName, item.GetType().GetProperty(registeredProperty.PropertyName).GetValue(item));
            }
        }

        public static void AddParameters<C>(DynamicParameters p, RegisteredClass<C> registeredClass, List<ExpressionPart>? expressionParts, string parameterPrefix = "w_")
        {
            if (expressionParts == null) return;

            foreach(var expression in expressionParts)
            {
                if (expression.ConstantExpression == null || expression.MemberExpression == null) continue;

                RegisteredProperty registeredProperty = registeredClass.RegisteredProperties.First(p => p.PropertyName == expression.MemberExpression.Member.Name);

                string parameterName = $"@{parameterPrefix}{registeredProperty.PropertyName}";

                if (expression.ConstantExpression.Value is ulong)
                {
                    p.Add(parameterName, Convert.ToInt64(expression.ConstantExpression.Value));
                }
                else if (expression.ConstantExpression.Value is ulong?) 
                {
                    p.Add(parameterName, Convert.ToInt64(expression.ConstantExpression.Value)); //todo fix this
                }
                else
                {
                    p.Add(parameterName, expression.ConstantExpression.Value);
                }
            }

            return;
        }

        public static void AddParameters<C>(DynamicParameters p, C item, IEnumerable<RegisteredProperty> registeredProperties, string parameterPrefix = "w_") //todo where C : notnull
        {
            if (item == null) throw new DbAutomatorException("The item cannot be null.", new ArgumentNullException("item"));

            foreach (var registeredProperty in registeredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement))
            {
                string parameterName = $"@{parameterPrefix}{registeredProperty.PropertyName}";

                p.AddDynamicParameter(parameterName, item, registeredProperty);
            }
        }

        public static BinaryExpression? GetBinaryExpression<C>(Expression<Func<C, object>>? expression)
        {
            if (expression == null) return null;

            UnaryExpression? unaryExpression = expression.Body as UnaryExpression;

            BinaryExpression? b1 = unaryExpression?.Operand as BinaryExpression;

            if (b1 == null) throw new DbAutomatorException("Unsupported expression", new ArgumentException());

            return b1;
        }

        public static List<ExpressionPart>? GetExpressionParts(BinaryExpression? binaryExpression, List<ExpressionPart>? result = null)
        {
            if (binaryExpression == null) return null;

            result ??= new List<ExpressionPart>();

            ExpressionPart wherePart = new ExpressionPart();

            if (binaryExpression.Left is BinaryExpression b1)
            {
                GetExpressionParts(b1, result);
            }

            if (binaryExpression.Left is BinaryExpression && binaryExpression.Right is BinaryExpression b2)
            {
                wherePart.NodeType = binaryExpression.NodeType;

                result.Add(wherePart);

                GetExpressionParts(b2, result);

                return result;
            }

            if (binaryExpression.Left is MemberExpression m1)
            {
                wherePart.MemberExpression = m1;
            }
            else if (binaryExpression.Left is UnaryExpression u1)
            {
                if (u1.Operand is MemberExpression m2)
                {
                    wherePart.MemberExpression = m2;
                }
            }
            else
            {
                throw new DbAutomatorException("Unsupported expression", new ArgumentException());
            }

            if (binaryExpression.Right is ConstantExpression c1)
            {
                wherePart.ConstantExpression = c1;
            }
            else
            {
                throw new DbAutomatorException("Unsupported expression", new ArgumentException());
            }

            wherePart.NodeType = binaryExpression.NodeType;

            result.Add(wherePart);

            return result;
        }

        public static bool IsStorable(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttributes<NotMappedAttribute>(true).FirstOrDefault() is NotMappedAttribute)
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

        /// <summary>
        /// "ColumnName" = @w_ColumnName
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <param name="registeredClass"></param>
        /// <param name="parameterPrefix"></param>
        /// <returns></returns>
        public static string ToColumnNameEqualsParameterName(IEnumerable<RegisteredProperty> registeredProperties, string parameterPrefix = "w_")
        {
            string result = string.Empty;

            foreach(RegisteredProperty registeredProperty in registeredProperties)
            {
                result = $"{result}\"{registeredProperty.ColumnName}\" = @{parameterPrefix}{registeredProperty.ColumnName}, ";
            }            

            return result[..^2];
        }

        /// <summary>
        /// MemberExpression.Member.Name = @w_MemberExpression.Member.Name
        /// </summary>
        /// <param name="expressionParts"></param>
        /// <param name="parameterPrefix"></param>
        /// <returns></returns>
        public static string ToColumnNameEqualsParameterName(List<ExpressionPart> expressionParts, string parameterPrefix = "w_")
        {
            string result = string.Empty;

            foreach (ExpressionPart expressionPart in expressionParts)
            {
                if (expressionPart.MemberExpression != null) result = $"{result} \"{expressionPart.MemberExpression?.Member.Name}\" ";

                result = $"{result}{expressionPart.NodeType.ToSqlSymbol()} ";

                if (expressionPart.MemberExpression != null) result = $"{result}@{parameterPrefix}{expressionPart.MemberExpression?.Member.Name} ";
            }

            result = result[..^1];

            return result;
        }

        public static string ToSqlSymbol(this ExpressionType expressionType) =>
            expressionType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.OrElse => "OR",
                ExpressionType.AndAlso => "AND",

                _ => throw new ArgumentException("Invalid enum value", nameof(expressionType))
            };



    }
}
