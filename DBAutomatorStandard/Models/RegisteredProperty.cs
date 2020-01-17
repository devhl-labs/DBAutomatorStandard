using System;
using System.Reflection;

namespace devhl.DBAutomator
{
    public class RegisteredProperty<C>
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public PropertyInfo Property { get; set; }

        public Type PropertyType { get; set; }

        public RegisteredClass<C> RegisteredClass { get; internal set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public string ColumnName { get; set; } = string.Empty;

        public string PropertyName { get; set; } = string.Empty;

        public bool IsKey { get; set; }

        public bool IsAutoIncrement { get; set; }

        public bool NotMapped { get; set; }

        /// <summary>
        /// This function controls how your data will be converted from a C# type to a type that can be stored in your database.
        /// </summary>
        public Func<RegisteredProperty<C>, object, object?> ToDatabaseColumn { get; set; } = DefaultToDatabaseColumn;

        public override string ToString() => PropertyName;

        public static object DefaultToDatabaseColumn(RegisteredProperty<C> registeredProperty, object value) => value;
    }
}
