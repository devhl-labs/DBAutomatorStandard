using System;
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

        public static void AddParameters<C>(DynamicParameters p, RegisteredClass<C> registeredClass, List<ExpressionPart<C>>? expressionParts, string parameterPrefix = "w_")
        {
            if (expressionParts == null) return;

            int i = 0;

            foreach(var expression in expressionParts)
            {
                if (expression.ConstantExpression == null || expression.MemberExpression == null) continue;

                RegisteredProperty<C> registeredProperty = GetRegisteredProperty(registeredClass, expression.MemberExpression);

                string parameterName = $"@{parameterPrefix}{i}";

                expression.ParameterName = i;

                i++;

                p.Add(parameterName, registeredProperty.ToDatabaseColumn(registeredProperty, expression.ConstantExpression.Value));
            }

            return;
        }

        public static void AddParameters<C>(DynamicParameters p, C item, IEnumerable<RegisteredProperty<C>> registeredProperties, string parameterPrefix = "w_")
        {
            if (item == null) throw new SqlWriterException("The item cannot be null.", new ArgumentNullException("item"));

            foreach (var registeredProperty in registeredProperties)
            {
                string parameterName = $"@{parameterPrefix}{registeredProperty.ColumnName}";

                p.Add(parameterName, registeredProperty.ToDatabaseColumn(registeredProperty, registeredProperty.Property.GetValue(item, null)));
            }
        }

        public static BinaryExpression GetBinaryExpression<C>(Expression<Func<C, object>> expression)
        {
            UnaryExpression? unaryExpression = expression.Body as UnaryExpression;

            BinaryExpression? b1 = unaryExpression?.Operand as BinaryExpression;

            if (b1 == null) throw new SqlWriterException("Unsupported expression", new ArgumentException());

            return b1;
        }

        public static List<ExpressionPart<C>> GetExpressionParts<C>(BinaryExpression binaryExpression, RegisteredClass<C> registeredClass, List<ExpressionPart<C>>? result = null, string parameterPrefix = "w_")
        {
            result ??= new List<ExpressionPart<C>>();

            ExpressionPart<C> wherePart = new ExpressionPart<C>();

            if (binaryExpression.Left is BinaryExpression b1)
            {
                result.Add(new ExpressionPart<C> { Parens = Parens.Left });

                GetExpressionParts(b1, registeredClass, result);

                result.Add(new ExpressionPart<C> { Parens = Parens.Right});
            }

            if (binaryExpression.Left is BinaryExpression && binaryExpression.Right is BinaryExpression b2)
            {
                wherePart.NodeType = binaryExpression.NodeType;

                result.Add(wherePart);

                result.Add(new ExpressionPart<C> { Parens = Parens.Left });

                GetExpressionParts(b2, registeredClass, result);

                result.Add(new ExpressionPart<C> { Parens = Parens.Right });

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

            if (binaryExpression.Right is ConstantExpression c1 && wherePart.MemberExpression != null)
            {
                wherePart.ConstantExpression = c1;

                wherePart.ConstantVariable = $"{registeredClass.RegisteredProperties.First(p => p.PropertyName == wherePart.MemberExpression.Member.Name).ColumnName}";

                wherePart.Prefix = parameterPrefix;

                wherePart.RegisteredProperty = registeredClass.RegisteredProperties.First(p => p.PropertyName == wherePart.MemberExpression.Member.Name);
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

            if (key.Body is UnaryExpression unaryExpression) member = unaryExpression.Operand as MemberExpression;

            if (member?.Member.Name == "Value" && member.Expression is MemberExpression member1) member = member1;

            if (member == null) throw new SqlWriterException("Unhandled expression type", new ArgumentException());

            return member;
        }
    }
}
