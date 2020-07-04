using System;
using System.Diagnostics;

namespace Dapper.SqlWriter.Models
{
    public class QueryFailure
    {
        public QueryFailure(string method, Exception e, string sql, Stopwatch stopwatch)
        {
            Method = method;

            Exception = e;

            Sql = sql;

            Stopwatch = stopwatch;
        }

        public DateTime DateTimeUTC => DateTime.UtcNow;

        public string Sql { get; }

        public string Method { get; }

        public Exception Exception { get; }

        public Stopwatch Stopwatch { get; }
    }
}
