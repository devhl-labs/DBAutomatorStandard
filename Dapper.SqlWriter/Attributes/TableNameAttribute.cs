using System;

namespace Dapper.SqlWriter
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]

    public class TableAttribute : Attribute
    {
        public string Name { get; }

        public TableAttribute(string name)
        {
            Name = name;
        }


    }
}
