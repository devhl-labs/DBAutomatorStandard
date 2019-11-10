using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace devhl.DBAutomator
{
    internal interface IUpdateQuery<C>
    {
        //Expression<Func<C, object>> Collection { get; }
        //List<ConditionModel> ConditionModels { get; }

        //Task<List<I>> GetListAsync();
        //Task<I> GetAsync();
        Task<IEnumerable<C>> UpdateAsync(Expression<Func<C, object>> setCollection, Expression<Func<C, object>>? whereCollection = null);

        Task<C> UpdateAsync(C item);
    }
}