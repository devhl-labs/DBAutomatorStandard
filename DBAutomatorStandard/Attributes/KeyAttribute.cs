using System;

namespace devhl.DBAutomator
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KeyAttribute : Attribute
    {
        public KeyAttribute()
        {

        }
    }
}
