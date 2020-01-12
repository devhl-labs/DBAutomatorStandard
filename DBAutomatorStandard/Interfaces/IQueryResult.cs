using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace devhl.DBAutomator.Interfaces
{
    public interface IQueryResult
    {
        DateTime DateTimeUTC { get; }

        string Sql { get; }

        string Method { get; }

        Stopwatch Stopwatch { get; }
    }
}
