using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.SqlWriter.Interfaces
{
    public interface ILogger
    {
        Task QueryExecuted(IQueryResult queryResult);
    }
}
