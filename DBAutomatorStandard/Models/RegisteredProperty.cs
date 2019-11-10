using System;
using System.Reflection;

namespace devhl.DBAutomator
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

        public bool IsAutoIncrement { get; set; }

    }
}
