using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.SqlWriter
{
    //https://stackoverflow.com/a/25938140/1578023
    public class ColumnMap
    {
        private readonly Dictionary<string, string> _mapping = new Dictionary<string, string>();

        public void Add(string propertyName, string columnName)
        {
            _mapping.Add(columnName, propertyName);
        }

        public string this[string index]
        {
            get
            {
                if (_mapping.ContainsKey(index)) return _mapping[index];

                //if nothing exists in this dictionary, assume the property name is the same as the column name
                return index;
            }
        }
    }
}
