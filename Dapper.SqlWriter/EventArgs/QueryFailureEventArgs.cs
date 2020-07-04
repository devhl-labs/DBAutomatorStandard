using System;
using Dapper.SqlWriter.Models;

namespace Dapper.SqlWriter
{
    public class QueryFailureEventArgs : EventArgs
    {
        public object Query { get; }

        public QueryFailure QueryFailure { get; }

        public QueryFailureEventArgs(object query, QueryFailure queryFailure)
        {
            Query = query;

            QueryFailure = queryFailure;
        }
    }
}
