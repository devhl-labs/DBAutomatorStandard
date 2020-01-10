using System;

namespace devhl.DBAutomator
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class NotMappedAttribute : Attribute
    {

        public NotMappedAttribute()
        {

        }


    }
}
