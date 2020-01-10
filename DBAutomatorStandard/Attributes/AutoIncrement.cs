using System;

namespace devhl.DBAutomator
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AutoIncrementAttribute : Attribute
    {
        public AutoIncrementAttribute()
        {

        }
    }
}
