using System;
using System.Collections.Generic;
using System.Text;

namespace DBAutomatorLibrary
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]

    public class TableNameAttribute : Attribute
    {
        public string TableName { get; }

        public TableNameAttribute(string tableName)
        {
            TableName = tableName;
        }


    }
}
