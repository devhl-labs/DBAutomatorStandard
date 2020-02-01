using System;

namespace Dapper.SqlWriter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AutoIncrementAttribute : Attribute
    {
        public AutoIncrementAttribute()
        {

        }
    }
}
