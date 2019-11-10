using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using static devhl.DBAutomator.Enums;

namespace devhl.DBAutomator
{
    public delegate void IsAvailableChangedEventHandler(bool isAvailable);

    public delegate void SlowQueryWarningEventHandler(string methodName, TimeSpan timeSpan);

    public class DBAutomator
    {
        public event SlowQueryWarningEventHandler? OnSlowQueryDetected;

        public ILogger? Logger { get; }

        private const string _source = nameof(DBAutomator);

        public readonly List<RegisteredClass> RegisteredClasses = new List<RegisteredClass>();

        public QueryOptions QueryOptions { get; }

        public DBAutomator(QueryOptions queryOptions, ILogger? logger = null)
        {
            Logger = logger;

            QueryOptions = queryOptions;
        }

        public void Register(object someObject)
        {
            try
            {
                var registeredClass = new RegisteredClass(someObject);

                RegisteredClasses.Add(registeredClass);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }


        internal void SlowQueryDetected(string methodName, TimeSpan timeSpan)
        {
            OnSlowQueryDetected?.Invoke(methodName, timeSpan);

            Logger?.LogWarning(LoggingEvents.SlowQuery, "{source}: Slow Query {methodName} took {seconds} seconds.", _source, methodName, (int)timeSpan.TotalSeconds);
        }




        public async Task<IEnumerable<C>> GetAsync<C>(Expression<Func<C, object>>? where = null, OrderByClause<C>? orderBy = null, QueryOptions? queryOptions = null)
        {
            try
            {
                queryOptions ??= QueryOptions;

                ISelectQuery<C> query;

                if (queryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresSelectQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.GetAsync(where, orderBy);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<C> InsertAsync<C>(C item, QueryOptions? queryOptions = null)
        {
            try
            {
                if (item == null)
                {
                    throw new NullReferenceException("The item cannot be null.");
                }

                queryOptions ??= QueryOptions;

                IInsertQuery<C> query;

                if (queryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresInsertQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.InsertAsync(item);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<IEnumerable<C>> DeleteAsync<C>(Expression<Func<C, object>>? where = null, QueryOptions? queryOptions = null)
        {
            try
            {
                queryOptions ??= QueryOptions;

                IDeleteQuery<C> query;

                if (queryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresDeleteQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.DeleteAsync(where);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<int> DeleteAsync<C>(C item, QueryOptions? queryOptions = null)
        {
            try
            {
                queryOptions ??= QueryOptions;

                IDeleteQuery<C> query;

                if (QueryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresDeleteQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.DeleteAsync(item);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<IEnumerable<C>> UpdateAsync<C>(Expression<Func<C, object>> set, Expression<Func<C, object>>? where = null, QueryOptions? queryOptions = null)
        {
            try
            {
                queryOptions ??= QueryOptions;

                IUpdateQuery<C> query;

                if (queryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresUpdateQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.UpdateAsync(set, where);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<C> UpdateAsync<C>(C item, QueryOptions? queryOptions = null)
        {
            try
            {
                queryOptions ??= QueryOptions;

                IUpdateQuery<C> query;

                if (queryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresUpdateQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.UpdateAsync(item);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }
    }
}
