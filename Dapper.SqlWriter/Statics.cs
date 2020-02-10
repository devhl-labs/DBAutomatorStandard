﻿using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;

using Dapper;

using Dapper.SqlWriter.Models;
using MiaPlaza.ExpressionUtils.Evaluating;
using MiaPlaza.ExpressionUtils;

namespace Dapper.SqlWriter
{
    internal static class Statics
    {
        public static string GetOperator(this Comparison comparison)
        {
            if (comparison == Comparison.Equals) return "=";

            if (comparison == Comparison.LessThan) return "<";

            if (comparison == Comparison.LessThanOrEqualTo) return "<=";

            if (comparison == Comparison.GreaterThan) return ">";

            if (comparison == Comparison.GreaterThanOrEqualTo) return ">=";

            throw new SqlWriterException("Unhandled value", new ArgumentException());
        }

        public static void AddParameters<C>(DynamicParameters p, RegisteredClass<C> registeredClass, List<ExpressionPart>? expressionParts, string parameterPrefix = "w_")
        {
            if (expressionParts == null) return;

            foreach(var expression in expressionParts)
            {
                if (expression.ConstantExpression == null || expression.MemberExpression == null) continue;

                RegisteredProperty<C> registeredProperty = GetRegisteredProperty(registeredClass, expression.MemberExpression);             

                string parameterName = $"@{parameterPrefix}{registeredProperty.PropertyName}";

                p.Add(parameterName, registeredProperty.ToDatabaseColumn(registeredProperty, expression.ConstantExpression.Value));
            }

            return;
        }

        public static void AddParameters<C>(DynamicParameters p, C item, IEnumerable<RegisteredProperty<C>> registeredProperties, string parameterPrefix = "w_")
        {
            if (item == null) throw new SqlWriterException("The item cannot be null.", new ArgumentNullException("item"));

            foreach (var registeredProperty in registeredProperties) //moved the where clause to higher
            {
                string parameterName = $"@{parameterPrefix}{registeredProperty.PropertyName}";

                p.Add(parameterName, registeredProperty.ToDatabaseColumn(registeredProperty, typeof(C).GetProperty(registeredProperty.PropertyName).GetValue(item, null)));
            }
        }

        public static BinaryExpression GetBinaryExpression<C>(Expression<Func<C, object>> expression)
        {
            UnaryExpression? unaryExpression = expression.Body as UnaryExpression;

            BinaryExpression? b1 = unaryExpression?.Operand as BinaryExpression;

            if (b1 == null) throw new SqlWriterException("Unsupported expression", new ArgumentException());

            return b1;
        }

        public static List<ExpressionPart> GetExpressionParts(BinaryExpression binaryExpression, List<ExpressionPart>? result = null, string parameterPrefix = "w_")
        {
            result ??= new List<ExpressionPart>();

            ExpressionPart wherePart = new ExpressionPart();

            if (binaryExpression.Left is BinaryExpression b1)
            {
                result.Add(new ExpressionPart { Parens = Parens.Left });

                GetExpressionParts(b1, result);

                result.Add(new ExpressionPart { Parens = Parens.Right});
            }

            if (binaryExpression.Left is BinaryExpression && binaryExpression.Right is BinaryExpression b2)
            {
                wherePart.NodeType = binaryExpression.NodeType;

                result.Add(wherePart);

                result.Add(new ExpressionPart { Parens = Parens.Left });

                GetExpressionParts(b2, result);

                result.Add(new ExpressionPart { Parens = Parens.Right });

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
                throw new SqlWriterException("Unsupported expression", new ArgumentException());
            }

            if (binaryExpression.Right is ConstantExpression c1)
            {
                wherePart.ConstantExpression = c1;

                wherePart.ConstantVariable = $"{parameterPrefix}{wherePart.MemberExpression.Member.Name}";
            }
            else
            {
                throw new SqlWriterException("Unsupported expression", new ArgumentException());
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
        public static string ToColumnNameEqualsParameterName<C>(IEnumerable<RegisteredProperty<C>> registeredProperties, string parameterPrefix = "w_", string delimiter = "")
        {
            string result = string.Empty;

            foreach(RegisteredProperty<C> registeredProperty in registeredProperties)
            {
                result = $"{result} \"{registeredProperty.ColumnName}\" = @{parameterPrefix}{registeredProperty.ColumnName}{delimiter}";
            }

            if (result.EndsWith(delimiter)) result = result[..^delimiter.Length];

            //return result[..^1];

            return result;
        }

        /// <summary>
        /// MemberExpression.Member.Name =&#60;&#62; @w_MemberExpression.Member.Name
        /// </summary>
        /// <param name="expressionParts"></param>
        /// <param name="parameterPrefix"></param>
        /// <returns></returns>
        public static string ToColumnNameEqualsParameterName<C>(RegisteredClass<C> registeredClass, List<ExpressionPart> expressionParts, string parameterPrefix = "w_")
        {
            string result = string.Empty;

            foreach (ExpressionPart expressionPart in expressionParts)
            {
                RegisteredProperty<C>? registeredProperty = null;

                if (expressionPart.Parens == Parens.Left) result = $"{result} ( ";

                if (expressionPart.MemberExpression != null)
                {
                    registeredProperty = GetRegisteredProperty(registeredClass, expressionPart.MemberExpression);
                }                

                if (registeredProperty != null) result = $"{result} \"{registeredProperty.ColumnName}\" ";

                if (expressionPart.MemberExpression != null && expressionPart.ConstantExpression?.Value == null)
                {
                    if (expressionPart.NodeType == ExpressionType.Equal)
                    {
                        result = $"{result}IS NULL ";
                    }
                    else if (expressionPart.NodeType == ExpressionType.NotEqual)
                    {
                        result = $"{result}IS NOT NULL ";
                    }
                    else
                    {
                        throw new SqlWriterException("Unhandled expression type", new ArgumentException());
                    }                    
                }
                else if (expressionPart.NodeType != null)
                {
                    result = $"{result}{expressionPart.NodeType.ToSqlSymbol()} ";

                    if (registeredProperty != null) result = $"{result}@{parameterPrefix}{registeredProperty.ColumnName} ";
                }

                if (expressionPart.Parens == Parens.Right) result = $"{result}) ";
            }

            result = result[..^1];

            if (result.StartsWith(" ")) result = result[1..];

            return result;
        }

        public static RegisteredProperty<C> GetRegisteredProperty<C>(RegisteredClass<C> registeredClass, MemberExpression expression)
        {
            RegisteredProperty<C> registeredProperty;

            if (expression.Member.Name == "Value" && expression.Expression is MemberExpression memberExpression)
            {
                registeredProperty = registeredClass.RegisteredProperties.First(p => p.PropertyName == ((MemberExpression) expression.Expression).Member.Name);
            }
            else if(expression.Expression is MemberExpression)
            {
                throw new SqlWriterException("Unhandled expression type", new ArgumentException());
            }
            else
            {
                registeredProperty = registeredClass.RegisteredProperties.First(p => p.PropertyName == expression.Member.Name);
            }

            return registeredProperty;
        }

        public static string ToSqlSymbol(this ExpressionType? expressionType) =>
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

                _ => throw new SqlWriterException("Invalid enum value", new ArgumentException())
            };

        public static MemberExpression GetMemberExpression<C>(Expression<Func<C, object>> key)
        {
            MemberExpression? member = key.Body as MemberExpression;

            if (key.Body is UnaryExpression unaryExpression)
            {
                member = unaryExpression.Operand as MemberExpression;
            }

            if (member?.Member.Name == "Value" && member.Expression is MemberExpression member1) member = member1;

            if (member == null) throw new SqlWriterException("Unhandled expression type", new ArgumentException());

            return member;
        }
    }
}
