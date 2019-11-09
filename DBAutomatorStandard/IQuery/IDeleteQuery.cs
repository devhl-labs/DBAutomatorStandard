using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DBAutomatorStandard
{
    internal interface IDeleteQuery <C> //<I, C>
    {
        Task<int> DeleteAsync(C item);

        Task<IEnumerable<C>> DeleteAsync(Expression<Func<C, object>>? where = null);
    }
}