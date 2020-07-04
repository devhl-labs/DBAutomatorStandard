using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;

using Dapper;

using Dapper.SqlWriter.Models;
using MiaPlaza.ExpressionUtils.Evaluating;
using MiaPlaza.ExpressionUtils;
using System.Threading.Tasks;

namespace Dapper.SqlWriter
{
    public enum Capitalization
    {
        Default,
        Upper,
        Lower
    }

    internal static class Utils
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

        public static void AddParameters<C>(DynamicParameters p, RegisteredClass<C> registeredClass, IEnumerable<IExpression> expressionParts, string parameterPrefix = "w_") where C : class
        {
            foreach(BodyExpression<C> expression in expressionParts.Where(p => p is BodyExpression<C>))
            {
                if (expression.ConstantExpression == null || expression.MemberExpression == null) 
                    continue;

                RegisteredProperty<C> registeredProperty = GetRegisteredProperty(registeredClass, expression.MemberExpression);

                string parameterName = $"@{parameterPrefix}{ expression.ParameterName }";

                p.Add(parameterName, ToDatabaseColumn(registeredProperty, expression.ConstantExpression.Value));
            }

            return;
        }

        public static object? ToDatabaseColumn<C>(RegisteredProperty<C> registeredProperty, object value) where C : class
        {
            if (registeredProperty.ToDatabaseColumn != null)            
                return registeredProperty.ToDatabaseColumn(registeredProperty, value);            
            else
            {
                PropertyMap? propertyMap = registeredProperty.SqlWriter.PropertyMaps.FirstOrDefault(pm => pm.PropertyType == registeredProperty.Property.PropertyType);

                if (propertyMap != null)                
                    return propertyMap.ToDatabaseColumn(value);                
                else               
                    return value;                
            }
        }

        public static void AddParameters<C>(DynamicParameters p, C item, IEnumerable<RegisteredProperty<C>> registeredProperties, string parameterPrefix = "w_") where C : class
        {
            foreach (var registeredProperty in registeredProperties)
            {
                string parameterName = $"@{parameterPrefix}{registeredProperty.ColumnName}";

                p.Add(parameterName, ToDatabaseColumn(registeredProperty, registeredProperty.Property.GetValue(item, null)));
            }
        }

        public static BinaryExpression GetBinaryExpression<C>(Expression<Func<C, object>> expression)
        {
            UnaryExpression? unaryExpression = expression.Body as UnaryExpression;

            if (!(unaryExpression?.Operand is BinaryExpression b1))
                throw new SqlWriterException("Unsupported expression", new ArgumentException());

            return b1;
        }

        public static List<IExpression> GetExpressionParts<C, T>(BaseQuery<C> sender, BinaryExpression binaryExpression, RegisteredClass<T> registeredClass, List<IExpression>? result = null, string parameterPrefix = "w_") where C : class where T : class
        {
            result ??= new List<IExpression>();

            MemberExpression? memberExpression = null;

            ExpressionType? nodeType = null;

            ConstantExpression? constantExpression = null;

            RegisteredProperty<T>? registeredProperty = null;

            if (binaryExpression.Left is BinaryExpression b1)
            {
                result.Add(new ParenthesisExpression(Parens.Left));

                GetExpressionParts(sender, b1, registeredClass, result, parameterPrefix);

                result.Add(new ParenthesisExpression(Parens.Right));
            }

            if (binaryExpression.Left is BinaryExpression && binaryExpression.Right is BinaryExpression b2)
            {
                result.Add(new NodeExpression(binaryExpression.NodeType));

                result.Add(new ParenthesisExpression(Parens.Left));

                GetExpressionParts(sender, b2, registeredClass, result, parameterPrefix);

                result.Add(new ParenthesisExpression(Parens.Right));

                return result;
            }

            if (binaryExpression.Left is MemberExpression m1)            
                memberExpression = m1;            
            else if (binaryExpression.Left is UnaryExpression u1)            
                if (u1.Operand is MemberExpression m2)                
                    memberExpression = m2;            
            else            
                throw new SqlWriterException("Unsupported expression", new ArgumentException());            

            if (binaryExpression.Right is ConstantExpression c1 && memberExpression != null)
            {
                constantExpression = c1;

                if (memberExpression.Expression is MemberExpression propertyIsNullable)                
                    registeredProperty = registeredClass.RegisteredProperties.First(p => p.Property.Name == propertyIsNullable.Member.Name);                
                else                
                    registeredProperty = registeredClass.RegisteredProperties.First(p => p.Property.Name == memberExpression.Member.Name);                
            }
            else if (binaryExpression.Right is UnaryExpression b && memberExpression != null /*wherePart.MemberExpression != null*/)            
                throw new SqlWriterException("Unsupported expression. The right hand side should be a constant. When comparing a nullable to a constant, use the nullable's Value property.", new ArgumentException());
            else            
                throw new SqlWriterException("Unsupported expression", new ArgumentException());            

            nodeType = binaryExpression.NodeType;

            BodyExpression<T> part = new BodyExpression<T>(registeredProperty, memberExpression, nodeType.Value, constantExpression, parameterPrefix, sender.ParameterIndex);

            sender.ParameterIndex++;

            result.Add(part);

            return result;
        }

        public static bool IsStorable(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttributes<NotMappedAttribute>(true).FirstOrDefault() is NotMappedAttribute)            
                return false;            

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

                propertyInfo.PropertyType.IsEnum ||
                propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && propertyInfo.PropertyType.GetGenericArguments()[0].IsEnum
                )            
                return true;
            
            return false;
        }

        public static RegisteredProperty<C> GetRegisteredProperty<C>(RegisteredClass<C> registeredClass, MemberExpression expression) where C : class
        {
            RegisteredProperty<C> registeredProperty;

            if (expression.Member.Name == "Value" && expression.Expression is MemberExpression memberExpression)            
                registeredProperty = registeredClass.RegisteredProperties.First(p => p.Property.Name == ((MemberExpression) expression.Expression).Member.Name);            
            else if(expression.Expression is MemberExpression)            
                throw new SqlWriterException("Unhandled expression type", new ArgumentException());            
            else            
                registeredProperty = registeredClass.RegisteredProperties.First(p => p.Property.Name == expression.Member.Name);            

            return registeredProperty;
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

                _ => throw new SqlWriterException("Invalid enum value", new ArgumentException())
            };

        public static MemberExpression GetMemberExpression<C>(Expression<Func<C, object>> key)
        {
            MemberExpression? member = key.Body as MemberExpression;

            if (key.Body is UnaryExpression unaryExpression) 
                member = unaryExpression.Operand as MemberExpression;

            if (member?.Member.Name == "Value" && member.Expression is MemberExpression member1)
                member = member1;

            if (member == null) 
                throw new SqlWriterException("Unhandled expression type", new ArgumentException());

            return member;
        }
 
        public static Expression<Func<C, object>> RemoveClosure<C>(this Expression<Func<C, object>> exp)
        {
            var a = PartialEvaluator.PartialEvalBody(exp, ExpressionInterpreter.Instance);

            if (a.ToString().Contains("An error occured during dynamic evaluation of an expression") == false)
                return a;

            return exp;
        }

        public static void FillDatabaseGeneratedProperties<C>(C? userInput, C? databaseOutput, SqlWriter sqlWriter) where C : class
        {
            if (userInput == null || databaseOutput == null)
                return;

            RegisteredClass<C> registeredClass = (RegisteredClass<C>) sqlWriter.RegisteredClasses.First(r => r is RegisteredClass<C>);

            foreach (RegisteredProperty<C> registeredProperty in registeredClass.RegisteredProperties.Where(p => p.IsAutoIncrement))
                registeredProperty.Property.SetValue(userInput, registeredProperty.Property.GetValue(databaseOutput, null));
        }
    }
}
