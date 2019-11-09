using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DBAutomatorStandard
{
    public class RegisteredProperty
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public PropertyInfo Property { get; set; }

        public Type PropertyType { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.



        public string ColumnName { get; set; } = string.Empty;

        public string PropertyName { get; set; } = string.Empty;

        public bool IsKey { get; set; }


    }


    public class RegisteredClass
    {
        public readonly object SomeClass;

        public readonly string TableName = string.Empty;

        public List<RegisteredProperty> RegisteredProperties { get; set; } = new List<RegisteredProperty>();

        public RegisteredClass(object someClass)
        {
            SomeClass = someClass;

            TableName = GetTableName(someClass);

            Dictionary<string, string> columnMaps = new Dictionary<string, string>();

            foreach (PropertyInfo property in someClass.GetType().GetProperties())
            {
                if (property.IsStorable())
                {
                    RegisteredProperty registeredProperty = new RegisteredProperty
                    {
                        Property = property,

                        ColumnName = GetColumnName(property),

                        IsKey = IsKey(property),

                        PropertyName = $"{property.Name}",

                        PropertyType = property.GetValue(someClass).GetType()
                    };

                    RegisteredProperties.Add(registeredProperty);

                    if (registeredProperty.PropertyName != registeredProperty.ColumnName)
                    {
                        columnMaps.Add(registeredProperty.ColumnName, registeredProperty.PropertyName);
                    }
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

            var map = new CustomPropertyTypeMap(
                someClass.GetType(),
                (type, columnName) => mapper(type, columnName)
            );

            SqlMapper.SetTypeMap(someClass.GetType(), map);


        }

        private bool IsKey(PropertyInfo property)
        {
            if (Attribute.IsDefined(property, typeof(KeyAttribute)))
            {
                return true;
            }

            return false;
        }

        private string GetTableName(object someClass)
        {
            string result = someClass.GetType().Name;

            if (someClass.GetType().GetCustomAttributes<TableAttribute>(true).FirstOrDefault() is TableAttribute tableNameAttribute)
            {
                result = tableNameAttribute.Name;
            }

            return $"{result}";
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
    }
}
