using System;

namespace Dapper.SqlWriter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KeyAttribute : Attribute
    {
        public KeyAttribute()
        {

        }
    }
}
