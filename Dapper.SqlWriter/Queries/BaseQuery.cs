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

        protected SqlWriter _sqlWriter;

        protected QueryOptions _queryOptions;

        protected RegisteredClass<C> _registeredClass;

        protected IDbConnection _connection;

#nullable enable

        protected ILogger? _logger;

        protected readonly DynamicParameters _p = new DynamicParameters();

        protected Stopwatch StopWatchStart()
        {
            Stopwatch result = new Stopwatch();

            result.Start();

            return result;
        }

        protected void StopWatchEnd(Stopwatch stopwatch, string methodName)
        {
            stopwatch.Stop();

            if (stopwatch.Elapsed > _queryOptions.SlowQueryWarning) _sqlWriter.SlowQueryDetected(methodName, stopwatch.Elapsed);
        }

        protected void PrepareResults(QueryType queryType, IEnumerable<C> results)
        {
            foreach (var result in results) PrepareResult(queryType, result);
        }

        protected void PrepareResult(QueryType queryType, C result)
        {
            if (result is DBObject dbObject)
            {
                dbObject.ObjectState = ObjectState.InDatabase;

                if (queryType == QueryType.Delete) dbObject.ObjectState = ObjectState.Deleted;

                dbObject.StoreState<C>(_sqlWriter);
            }
        }



        protected async Task<IEnumerable<C>> QueryAsync(QueryType queryType, string sql)
        {
            Stopwatch stopwatch = StopWatchStart();

            IEnumerable<C> result;

            try
            { 
                await _sqlWriter._semaphoreSlim.WaitAsync();

                result = await _connection.QueryAsync<C>(sql, _p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                _logger?.QueryExecuted(new QuerySuccess { Method = "QueryAsync", Results = result.Count(), Sql = sql, Stopwatch = stopwatch });
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                _logger?.QueryExecuted(new QueryFailure { Method = "QueryAsync", Exception = error, Sql = sql, Stopwatch = stopwatch });

                throw error;
            }
            finally
            {
                _sqlWriter._semaphoreSlim.Release();
            }

            PrepareResults(queryType, result);

            return result;
        }

        protected async Task<C> QueryFirstAsync(QueryType queryType, string sql)
        {
            Stopwatch stopwatch = StopWatchStart();

            C result;

            try
            {
                await _sqlWriter._semaphoreSlim.WaitAsync();

                result = await _connection.QueryFirstAsync<C>(sql, _p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                _logger?.QueryExecuted(new QuerySuccess { Method = "QueryFirstAsync", Results = 1, Sql = sql, Stopwatch = stopwatch });
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                _logger?.QueryExecuted(new QueryFailure { Method = "QueryFirstAsync", Exception = error, Sql = sql, Stopwatch = stopwatch });

                throw error;
            }
            finally
            {
                _sqlWriter._semaphoreSlim.Release();
            }

            PrepareResult(queryType, result);

            return result;
        }

        protected async Task<C?> QueryFirstOrDefaultAsync(QueryType queryType, string sql)
        {
            Stopwatch stopwatch = StopWatchStart();

            C? result;

            try
            {
                await _sqlWriter._semaphoreSlim.WaitAsync();

                result = await _connection.QueryFirstOrDefaultAsync<C>(sql, _p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                _logger?.QueryExecuted(new QuerySuccess { Method = "QueryFirstOrDefaultAsync", Results = 1, Sql = sql, Stopwatch = stopwatch });

            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                _logger?.QueryExecuted(new QueryFailure { Method = "QueryFirstOrDefaultAsync", Exception = error, Sql = sql, Stopwatch = stopwatch });

                throw error;
            }
            finally
            {
                _sqlWriter._semaphoreSlim.Release();
            }

            PrepareResult(queryType, result);

            return result;
        }

        protected async Task<C> QuerySingleAsync(QueryType queryType, string sql)
        {
            Stopwatch stopwatch = StopWatchStart();

            C result;

            try
            {
                await _sqlWriter._semaphoreSlim.WaitAsync();

                result = await _connection.QuerySingleAsync<C>(sql, _p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                _logger?.QueryExecuted(new QuerySuccess { Method = "QuerySingleAsync", Results = 1, Sql = sql, Stopwatch = stopwatch });
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                _logger?.QueryExecuted(new QueryFailure { Method = "QuerySingleAsync", Exception = error, Sql = sql, Stopwatch = stopwatch });

                throw error;
            }
            finally
            {
                _sqlWriter._semaphoreSlim.Release();
            }

            PrepareResult(queryType, result);

            return result;
        }

        protected async Task<C?> QuerySingleOrDefaultAsync(QueryType queryType, string sql)
        {
            Stopwatch stopwatch = StopWatchStart();

            C? result;

            try
            {
                await _sqlWriter._semaphoreSlim.WaitAsync();

                result = await _connection.QuerySingleOrDefaultAsync<C>(sql, _p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                _logger?.QueryExecuted(new QuerySuccess { Method = "QuerySingleOrDefaultAsync", Results = 1, Sql = sql, Stopwatch = stopwatch });
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                _logger?.QueryExecuted(new QueryFailure { Method = "QuerySingleOrDefaultAsync", Exception = error, Sql = sql, Stopwatch = stopwatch });

                throw error;
            }
            finally
            {
                _sqlWriter._semaphoreSlim.Release();
            }

            PrepareResult(queryType, result);

            return result;
        }




        private SqlWriterException GetException(Exception e) => new SqlWriterException("Error executing query.", e);
    }
}
