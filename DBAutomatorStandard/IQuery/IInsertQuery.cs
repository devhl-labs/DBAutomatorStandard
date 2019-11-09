using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DBAutomatorStandard
{
    internal interface IInsertQuery
    {
        Task<int> InsertAsync(object item);
    }
}