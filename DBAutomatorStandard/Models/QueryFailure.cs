using devhl.DBAutomator.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace devhl.DBAutomator.Models
{
    public class QueryFailure : IQueryResult
    {
#nullable disable

        public DateTime DateTimeUTC => DateTime.UtcNow;

        public string Sql { get; internal set; }

        public string Method { get; internal set; }

        public Exception Exception { get; internal set; }

        public Stopwatch Stopwatch { get; internal set; }
    }
}
