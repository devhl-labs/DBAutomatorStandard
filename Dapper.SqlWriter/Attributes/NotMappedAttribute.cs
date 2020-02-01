using System;

namespace Dapper.SqlWriter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class NotMappedAttribute : Attribute
    {

        public NotMappedAttribute()
        {

        }


    }
}
