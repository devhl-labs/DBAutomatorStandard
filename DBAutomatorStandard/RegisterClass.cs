using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using DBAutomatorLibrary;
using System.Linq;

namespace DBAutomatorStandard
{
    public class RegisteredProperty
    {
        public PropertyInfo Property { get; set; }

        public string ColumnName { get; set; } = string.Empty;

        public string PropertyName { get; set; }

        public bool IsKey { get; set; }

        public Type PropertyType { get; set; }
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
                }
            }
        }

        private bool IsKey(PropertyInfo property)
        {
            if (Attribute.IsDefined(property, typeof(IdentityAttribute)))
            {
                return true;
            }

            return false;
        }

        private string GetTableName(object someClass)
        {
            string result = someClass.GetType().Name;

            if (someClass.GetType().GetCustomAttributes<TableNameAttribute>(true).FirstOrDefault() is TableNameAttribute tableNameAttribute)
            {
                result = tableNameAttribute.TableName;
            }

            return $"\"{result}\"";
            //return result;
        }

        public string GetColumnName(PropertyInfo property)
        {
            string result = property.Name;

            if (property.GetCustomAttributes<ColumnNameAttribute>(true).FirstOrDefault() is ColumnNameAttribute columnNameAttribute)
            {
                result = columnNameAttribute.ColumnName;
            }

            //return $"\"{result}\"";
            return result;
        }
    }
}
