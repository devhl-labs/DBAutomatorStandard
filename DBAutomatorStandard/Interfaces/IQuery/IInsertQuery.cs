using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace devhl.DBAutomator
{
    internal interface IInsertQuery <C>
    {
        Task<C> InsertAsync(C item);
    }
}