using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.SqlWriter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public readonly string Name;

        public ColumnAttribute(string name)
        {
            Name = name;
        }


    }
}
