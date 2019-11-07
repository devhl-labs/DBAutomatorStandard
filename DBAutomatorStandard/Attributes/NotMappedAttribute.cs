using System;
using System.Collections.Generic;
using System.Text;

namespace DBAutomatorLibrary
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NotMappedAttribute : Attribute
    {

        public NotMappedAttribute()
        {

        }


    }
}
