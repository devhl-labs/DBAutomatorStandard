using System;
using System.Collections.Generic;
using System.Text;

namespace DBAutomatorStandard
{
    public class LoggingEvents
    {
        public const int QueryingDatabase = 0;
        public const int ModifyingDatabase = 1;
        public const int SlowQuery = 2;
        public const int ErrorExecutingQuery = 3;

    }
    
}
