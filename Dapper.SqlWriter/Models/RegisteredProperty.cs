using System;
using System.Reflection;

namespace Dapper.SqlWriter
{
    public class RegisteredProperty<C>
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public PropertyInfo Property { get; set; }

        public Type PropertyType { get; set; }

        public RegisteredClass<C> RegisteredClass { get; internal set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public string ColumnName { get; internal set; } = string.Empty;

        public string PropertyName { get; set; } = string.Empty;

        public bool IsKey { get; set; }

        public bool IsAutoIncrement { get; set; }

        public bool NotMapped { get; set; }

        /// <summary>
        /// This function controls how your data will be converted from a C# type to a type that can be stored in your database.
        /// </summary>
        public Func<RegisteredProperty<C>, object, object?> ToDatabaseColumn { get; set; } = DefaultToDatabaseColumn;

        public override string ToString() => ToString("w_");

        public string ToString(string parameterPrefix) => $"\"{ColumnName}\" = @{parameterPrefix}{ColumnName}";

        public string ToSqlInjectionString(C item)
        {
            if (PropertyType.GetType() == typeof(string))
            {
                return $"\"{ColumnName}\" = '{Property.GetValue(item, null)}'";
            }
            else
            {
                return $"\"{ColumnName}\" = {Property.GetValue(item, null)}";
            }

        }

        public static object DefaultToDatabaseColumn(RegisteredProperty<C> registeredProperty, object value) => value;
    }
}
