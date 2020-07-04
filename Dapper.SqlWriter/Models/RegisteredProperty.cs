using System;
using System.Linq;
using System.Reflection;

namespace Dapper.SqlWriter
{
    public class RegisteredProperty<C> where C : class
    {
        public RegisteredProperty(RegisteredClass<C> registeredClass, PropertyInfo propertyInfo, SqlWriter sqlWriter)
        {
            RegisteredClass = registeredClass;

            Property = propertyInfo;

            SqlWriter = sqlWriter;
        }

        public PropertyInfo Property { get; }

        public RegisteredClass<C> RegisteredClass { get; }

        public string ColumnName { get; internal set; } = string.Empty;

        public bool IsKey { get; internal set; }

        public bool IsAutoIncrement { get; internal set; }

        public bool NotMapped { get; internal set; }

        /// <summary>
        /// This function controls how your data will be converted from a C# type to a type that can be stored in your database.
        /// </summary>
        public Func<RegisteredProperty<C>, object, object?>? ToDatabaseColumn { get; set; }

        public override string ToString() => ToString("w_");

        public string ToString(string parameterPrefix) => $"\"{ColumnName}\" = @{parameterPrefix}{ColumnName}";

        public string ToSqlInjectionString(C item)
        {
            if (Property.PropertyType.GetType() == typeof(string))            
                return $"\"{ColumnName}\" = '{Property.GetValue(item, null)}'";            
            else           
                return $"\"{ColumnName}\" = {Property.GetValue(item, null)}";            
        }

        public SqlWriter SqlWriter { get; }

    }
}
