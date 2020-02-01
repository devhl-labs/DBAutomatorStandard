using Dapper.SqlWriter.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Dapper.SqlWriter.Models
{
#nullable disable

    public class QuerySuccess : IQueryResult
    {
        public DateTime DateTimeUTC => DateTime.UtcNow;

        public string Sql { get; internal set; }

        public string Method { get; internal set; }

        public long Results { get; internal set; }

        public Stopwatch Stopwatch { get; internal set; }
    }
}
