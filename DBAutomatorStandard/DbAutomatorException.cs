using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.DBAutomator
{
    [Serializable]
    public class DbAutomatorException : Exception
    {
        public DbAutomatorException(string message, Exception exception) : base(message, exception)
        {

        }

    }
}
