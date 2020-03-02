using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Dapper;
using System.Linq.Expressions;
using MiaPlaza.ExpressionUtils;
using MiaPlaza.ExpressionUtils.Evaluating;

namespace Dapper.SqlWriter
{
    public class RegisteredClass<C> where C : class
    {
        internal string DatabaseTableName { get; set; } = string.Empty;

        public List<RegisteredProperty<C>> RegisteredProperties { get; set; } = new List<RegisteredProperty<C>>();

        public RegisteredClass()
        {
            DatabaseTableName = GetTableName();

            Dictionary<string, string> columnMaps = new Dictionary<string, string>();

            foreach (PropertyInfo property in typeof(C).GetProperties())
            {
                RegisteredProperty<C> registeredProperty = new RegisteredProperty<C>
                {
                    Property = property,

                    ColumnName = GetColumnName(property),

                    IsKey = IsKey(property),

                    IsAutoIncrement = IsAutoIncrement(property),

                    PropertyName = $"{property.Name}",

                    PropertyType = property.PropertyType,

                    NotMapped = !property.IsStorable()                    
                };

                registeredProperty.RegisteredClass = this;

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

            var map = new CustomPropertyTypeMap(typeof(C), (type, columnName) => mapper(type, columnName));

            SqlMapper.SetTypeMap(typeof(C), map);
        }

        public RegisteredClass<C> NotMapped(Expression<Func<C, object>> key)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression member = Statics.GetMemberExpression(key);

            RegisteredProperties.First(p => p.PropertyName == member.Member.Name).NotMapped = true;

            return this;
        }

        public RegisteredClass<C> Key(Expression<Func<C, object>> key)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression member = Statics.GetMemberExpression(key);

            RegisteredProperties.First(p => p.PropertyName == member.Member.Name).IsKey = true;

            return this;
        }

        public RegisteredClass<C> AutoIncrement(Expression<Func<C, object>> key)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression member = Statics.GetMemberExpression(key);

            RegisteredProperties.First(p => p.PropertyName == member.Member.Name).IsAutoIncrement = true;

            return this;
        }

        public RegisteredClass<C> TableName(string tableName)
        {
            DatabaseTableName = tableName;

            return this;
        }

        private ColumnMap? _columnMap = null;

        public RegisteredClass<C> ColumnName(Expression<Func<C, object>> key, string columnName)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression member = Statics.GetMemberExpression(key);

            RegisteredProperties.First(p => p.PropertyName == member.Member.Name).ColumnName = columnName;

            _columnMap ??= new ColumnMap();

            _columnMap.Add(member.Member.Name, columnName);

            SqlMapper.SetTypeMap(typeof(C), new CustomPropertyTypeMap(typeof(C), (type, columnName) => type.GetProperty(_columnMap[columnName])));

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
            string result = typeof(C).Name;

            if (typeof(C).GetCustomAttributes<TableAttribute>(true).FirstOrDefault() is TableAttribute tableNameAttribute)
            {
                result = tableNameAttribute.Name;
            }

            return result;
        }

        private string GetColumnName(PropertyInfo property)
        {
            string result = property.Name;

            if (property.GetCustomAttributes<ColumnAttribute>(true).FirstOrDefault() is ColumnAttribute columnNameAttribute)
            {
                result = columnNameAttribute.Name;
            }

            return result;
        }

        public RegisteredProperty<C> RegisteredProperty(Expression<Func<C, object>> key)
        {
            key = PartialEvaluator.PartialEvalBody(key, ExpressionInterpreter.Instance);

            MemberExpression memberExpression = Statics.GetMemberExpression(key);

            return RegisteredProperties.First(p => p.PropertyName == memberExpression.Member.Name);
        }
    }
}
