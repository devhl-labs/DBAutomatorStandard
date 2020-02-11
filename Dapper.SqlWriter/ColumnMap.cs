using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.SqlWriter
{
    //https://stackoverflow.com/a/25938140/1578023
    public class ColumnMap
    {
        private readonly Dictionary<string, string> _forward = new Dictionary<string, string>();

        private readonly Dictionary<string, string> _reverse = new Dictionary<string, string>();

        public void Add(string t1, string t2)
        {
            _forward.Add(t1, t2);

            _reverse.Add(t2, t1);
        }

        public string this[string index]
        {
            get
            {
                if (_forward.ContainsKey(index)) return _forward[index];

                if (_reverse.ContainsKey(index)) return _reverse[index];

                return index;
            }
        }
    }
}
