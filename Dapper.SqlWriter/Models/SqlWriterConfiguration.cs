using System;
using System.Data;

namespace Dapper.SqlWriter
{
    public class SqlWriterConfiguration
    {
        public TimeSpan SlowQueryWarning { get; set; } = TimeSpan.FromSeconds(5);

        public IDbTransaction? DbTransaction { get; set; }

        public int? CommandTimeOut { get; set; }
    }
}
