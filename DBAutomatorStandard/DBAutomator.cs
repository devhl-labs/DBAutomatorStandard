using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MiaPlaza.ExpressionUtils;
using MiaPlaza.ExpressionUtils.Evaluating;
using Microsoft.Extensions.Logging;

using static devhl.DBAutomator.Enums;

namespace devhl.DBAutomator
{
    public delegate void IsAvailableChangedEventHandler(bool isAvailable);

    public delegate void SlowQueryWarningEventHandler(string methodName, TimeSpan timeSpan);

    public class DBAutomator
    {
        public event SlowQueryWarningEventHandler? OnSlowQueryDetected;

        public ILogger? Logger { get; set; }

        private const string _source = nameof(DBAutomator);

        public readonly List<object> RegisteredClasses = new List<object>();

        public QueryOptions QueryOptions { get; set; } = new QueryOptions();

        public DBAutomator(QueryOptions queryOptions, ILogger? logger = null)
        {
            Logger = logger;

            QueryOptions = queryOptions;
        }

        public RegisteredClass<T> Register<T>()
        {
            try
            {
                var registeredClass = new RegisteredClass<T>();

                RegisteredClasses.Add(registeredClass);

                return registeredClass;
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }


        }

        public RegisteredClass<T> Register<T>(string tableName)
        {
            var item = Register<T>();

            item.TableName = tableName;

            return item;            
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
                if (where != null) where = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

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

                return await query.GetAsync(where, orderBy).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<C> GetFirstOrDefaultAsync<C>(Expression<Func<C, object>>? where = null, OrderByClause<C>? orderBy = null, QueryOptions? queryOptions = null)
        {
            try
            {
                var result = await GetAsync(where, orderBy, queryOptions).ConfigureAwait(false);

                return result.ToList().FirstOrDefault();               

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

                return await query.InsertAsync(item).ConfigureAwait(false);
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
                if (where != null) where = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

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

                return await query.DeleteAsync(where).ConfigureAwait(false);
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

                return await query.DeleteAsync(item).ConfigureAwait(false);
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
                set = PartialEvaluator.PartialEvalBody(set, ExpressionInterpreter.Instance);

                if (where != null) where = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

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

                return await query.UpdateAsync(set, where).ConfigureAwait(false);
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

                return await query.UpdateAsync(item).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }
    }
}
