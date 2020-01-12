using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace devhl.DBAutomator.Interfaces
{
    public interface ILogger
    {
        Task QueryExecuted(IQueryResult queryResult);
    }
}
