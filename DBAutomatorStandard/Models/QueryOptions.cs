using System;
using System.Data;

namespace devhl.DBAutomator
{
    public class QueryOptions
    {
        public TimeSpan SlowQueryWarning { get; set; } = TimeSpan.FromSeconds(5);

        public IDbTransaction? DbTransaction { get; set; }

        public int? CommandTimeOut { get; set; }
    }
}
