using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Dapper;
using System.Linq.Expressions;
using MiaPlaza.ExpressionUtils;
using MiaPlaza.ExpressionUtils.Evaluating;

namespace devhl.DBAutomator
{
    public class RegisteredClass<T>
    {
        public string TableName { get; set; } = string.Empty;

        public List<RegisteredProperty> RegisteredProperties { get; set; } = new List<RegisteredProperty>();

        public RegisteredClass()
        {
            TableName = GetTableName();

            Dictionary<string, string> columnMaps = new Dictionary<string, string>();

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                RegisteredProperty registeredProperty = new RegisteredProperty
                {
                    Property = property,

                    ColumnName = GetColumnName(property),

                    IsKey = IsKey(property),

                    IsAutoIncrement = IsAutoIncrement(property),

                    PropertyName = $"{property.Name}",

                    PropertyType = property.PropertyType,

                    NotMapped = !property.IsStorable()
                };

                RegisteredProperties.Add(registeredProperty);

                if (registeredProperty.PropertyName != registeredProperty.ColumnName)
                {
                    columnMaps.Add(registeredProperty.ColumnName, registeredProperty.PropertyName);
                }
            }

            var mapper = new Func<Type, string, PropertyInfo>((type, columnName) =>
            {
                if (columnMaps.ContainsKey(columnName))
                {
                    return type.GetProperty(columnMaps[columnName]);
                }
                else
                {
                    return type.GetProperty(columnName);
                }
            });

            var map = new CustomPropertyTypeMap(typeof(T), (type, columnName) => mapper(type, columnName));

            SqlMapper.SetTypeMap(typeof(T), map);
        }

        public string GetColumnName(PropertyInfo property)
        {
            string result = property.Name;

            if (property.GetCustomAttributes<ColumnAttribute>(true).FirstOrDefault() is ColumnAttribute columnNameAttribute)
            {
                result = columnNameAttribute.Name;
            }

            return result;
        }

        public RegisteredClass<T> NotMapped(Expression<Func<T, object>> ignore)
        {
            string paramater = ignore.Parameters.First().Name;

            int start = paramater.Length + 1;

            string propertyName = ignore.Body.ToString()[start..];

            RegisteredProperties.First(p => p.PropertyName == propertyName).NotMapped = true;

            return this;
        }

        public RegisteredClass<T> Key(Expression<Func<T, object>> key)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression? member = key.Body as MemberExpression;

            if (key.Body is UnaryExpression unaryExpression)
            {
                member = unaryExpression.Operand as MemberExpression;
            }

            if (member == null) throw new DbAutomatorException("Unhandled expression type", new ArgumentException());


            RegisteredProperties.First(p => p.PropertyName == member.Member.Name).IsKey = true;

            return this;
        }

        public RegisteredClass<T> AutoIncrement(Expression<Func<T, object>> key)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression? member = key.Body as MemberExpression;

            if (key.Body is UnaryExpression unaryExpression)
            {
                member = unaryExpression.Operand as MemberExpression;
            }

            if (member == null) throw new DbAutomatorException("Unhandled expression type", new ArgumentException());


            RegisteredProperties.First(p => p.PropertyName == member.Member.Name).IsAutoIncrement = true;

            return this;
        }

        public RegisteredClass<T> ColumnName(Expression<Func<T, object>> key)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            UnaryExpression? unaryExpression = key.Body as UnaryExpression;

            BinaryExpression? binaryExpression = unaryExpression?.Operand as BinaryExpression;

            MemberExpression? member = binaryExpression?.Left as MemberExpression;

            member ??= binaryExpression?.Right as MemberExpression;

            ConstantExpression? constant = binaryExpression?.Right as ConstantExpression;

            constant ??= binaryExpression?.Left as ConstantExpression;

            if (member == null || constant == null) throw new DbAutomatorException("Unhandled expression type", new ArgumentException());

            RegisteredProperties.First(p => p.PropertyName == member.Member.Name).ColumnName = constant.Value.ToString();

            return this;
        }

        private bool IsAutoIncrement(PropertyInfo property)
        {
            if (Attribute.IsDefined(property, typeof(AutoIncrementAttribute)))
            {
                return true;
            }

            return false;
        }

        private bool IsKey(PropertyInfo property)
        {
            if (Attribute.IsDefined(property, typeof(KeyAttribute)))
            {
                return true;
            }

            return false;
        }

        private string GetTableName()
        {
            string result = typeof(T).Name;

            if (typeof(T).GetCustomAttributes<TableAttribute>(true).FirstOrDefault() is TableAttribute tableNameAttribute)
            {
                result = tableNameAttribute.Name;
            }

            return $"{result}";
        }
    }
}
