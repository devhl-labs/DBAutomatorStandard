using Dapper.SqlWriter.Interfaces;
using Dapper.SqlWriter.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.SqlWriter
{
    public abstract class BaseQuery<C> where C : class
    {
#nullable disable

        protected SqlWriter SqlWriter { get; set; }

        protected QueryOptions QueryOptions { get; set; }

        protected RegisteredClass<C> RegisteredClass { get; set; }

        protected IDbConnection Connection { get; set; }

#nullable enable

        protected ILogger? Logger { get; set; }

        protected DynamicParameters P { get; set; } = new DynamicParameters();

        protected Stopwatch StopWatchStart()
        {
            Stopwatch result = new Stopwatch();

            result.Start();

            return result;
        }

        protected void StopWatchEnd(Stopwatch stopwatch, string sql)
        {
            stopwatch.Stop();

            if (stopwatch.Elapsed > QueryOptions.SlowQueryWarning) //SqlWriter.SlowQueryDetected(methodName, stopwatch.Elapsed);
                SqlWriter.OnSlowQuery(this, stopwatch.Elapsed, sql);
        }

        protected void PrepareResults(QueryType queryType, IEnumerable<C> results)
        {
            foreach (var result in results)
                PrepareResult(queryType, result);
        }

        protected void PrepareResult(QueryType queryType, C result)
        {
            if (result is DBObject dbObject)
            {
                dbObject.ObjectState = ObjectState.InDatabase;

                if (queryType == QueryType.Delete)
                    dbObject.ObjectState = ObjectState.Deleted;

                dbObject.StoreState<C>(SqlWriter);

                dbObject.QueryType = queryType;
            }
        }

        protected async Task<IEnumerable<C>> QueryAsync(QueryType queryType, string sql)
        {
            Stopwatch stopwatch = StopWatchStart();

            IEnumerable<C> result;

            try
            { 
                await SqlWriter.SemaphoreSlim.WaitAsync();

                result = await Connection.QueryAsync<C>(sql, P, QueryOptions.DbTransaction, QueryOptions.CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                Logger?.QueryExecuted(new QuerySuccess { Method = "QueryAsync", Results = result.Count(), Sql = sql, Stopwatch = stopwatch });

                PrepareResults(queryType, result);

                return result;
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                Logger?.QueryExecuted(new QueryFailure { Method = "QueryAsync", Exception = error, Sql = sql, Stopwatch = stopwatch });

                throw error;
            }
            finally
            {
                SqlWriter.SemaphoreSlim.Release();
            }
        }

        protected async Task<C> QueryFirstAsync(QueryType queryType, string sql)
        {
            Stopwatch stopwatch = StopWatchStart();

            C result;

            try
            {
                await SqlWriter.SemaphoreSlim.WaitAsync();

                result = await Connection.QueryFirstAsync<C>(sql, P, QueryOptions.DbTransaction, QueryOptions.CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                Logger?.QueryExecuted(new QuerySuccess { Method = "QueryFirstAsync", Results = 1, Sql = sql, Stopwatch = stopwatch });

                PrepareResult(queryType, result);

                return result;
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                Logger?.QueryExecuted(new QueryFailure { Method = "QueryFirstAsync", Exception = error, Sql = sql, Stopwatch = stopwatch });

                throw error;
            }
            finally
            {
                SqlWriter.SemaphoreSlim.Release();
            }
        }

        protected async Task<C?> QueryFirstOrDefaultAsync(QueryType queryType, string sql)
        {
            Stopwatch stopwatch = StopWatchStart();

            C? result;

            try
            {
                await SqlWriter.SemaphoreSlim.WaitAsync();

                result = await Connection.QueryFirstOrDefaultAsync<C>(sql, P, QueryOptions.DbTransaction, QueryOptions.CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                Logger?.QueryExecuted(new QuerySuccess { Method = "QueryFirstOrDefaultAsync", Results = 1, Sql = sql, Stopwatch = stopwatch });

                PrepareResult(queryType, result);

                return result;
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                Logger?.QueryExecuted(new QueryFailure { Method = "QueryFirstOrDefaultAsync", Exception = error, Sql = sql, Stopwatch = stopwatch });

                throw error;
            }
            finally
            {
                SqlWriter.SemaphoreSlim.Release();
            }
        }

        protected async Task<C> QuerySingleAsync(QueryType queryType, string sql)
        {
            Stopwatch stopwatch = StopWatchStart();

            C result;

            try
            {
                await SqlWriter.SemaphoreSlim.WaitAsync();

                result = await Connection.QuerySingleAsync<C>(sql, P, QueryOptions.DbTransaction, QueryOptions.CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                Logger?.QueryExecuted(new QuerySuccess { Method = "QuerySingleAsync", Results = 1, Sql = sql, Stopwatch = stopwatch });

                PrepareResult(queryType, result);

                return result;
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                Logger?.QueryExecuted(new QueryFailure { Method = "QuerySingleAsync", Exception = error, Sql = sql, Stopwatch = stopwatch });

                throw error;
            }
            finally
            {
                SqlWriter.SemaphoreSlim.Release();
            }
        }

        protected async Task<C?> QuerySingleOrDefaultAsync(QueryType queryType, string sql)
        {
            Stopwatch stopwatch = StopWatchStart();

            C? result;

            try
            {
                await SqlWriter.SemaphoreSlim.WaitAsync();

                result = await Connection.QuerySingleOrDefaultAsync<C>(sql, P, QueryOptions.DbTransaction, QueryOptions.CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                Logger?.QueryExecuted(new QuerySuccess { Method = "QuerySingleOrDefaultAsync", Results = 1, Sql = sql, Stopwatch = stopwatch });

                PrepareResult(queryType, result);

                return result;
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                Logger?.QueryExecuted(new QueryFailure { Method = "QuerySingleOrDefaultAsync", Exception = error, Sql = sql, Stopwatch = stopwatch });

                throw error;
            }
            finally
            {
                SqlWriter.SemaphoreSlim.Release();
            }
        }




        private SqlWriterException GetException(Exception e) => new SqlWriterException("Error executing query.", e);
    }
}
