using System;

namespace Dapper.SqlWriter
{
    public class SlowQueryEventArgs : EventArgs 
    {
        public object Query { get; }

        public TimeSpan TimeSpan { get; }

        public string Sql { get; }

        public SlowQueryEventArgs(object query, TimeSpan timeSpan, string sql)
        {
            Query = query;

            TimeSpan = timeSpan;

            Sql = sql;
        }
    }
}
