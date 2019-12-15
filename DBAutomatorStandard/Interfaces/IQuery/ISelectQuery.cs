using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace devhl.DBAutomator
{
    internal interface ISelectQuery<C>
    {
        Task<IEnumerable<C>> GetAsync(Expression<Func<C, object>>? where = null, OrderByClause<C>? orderBy = null);

        //Task<C> GetFirstOrDefaultAsync(Expression<Func<C, object>>? where = null, OrderByClause<C>? orderBy = null);
    }
}