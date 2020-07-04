using Dapper.SqlWriter.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dapper.SqlWriter
{
    public abstract class BaseQuery<C> where C : class
    {
#nullable disable

        protected SqlWriter SqlWriter { get; set; }

        protected RegisteredClass<C> RegisteredClass { get; set; }

#nullable enable

        public int ParameterIndex { get; set; }

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

            if (stopwatch.Elapsed > SqlWriter.Config.SlowQueryWarning)
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

        protected async Task<IEnumerable<C>> QueryAsync(QueryType queryType, string sql, string splitOn = "")
        {
            if (SqlWriter.Config.AllowConcurrentQueries == false)
                await SqlWriter.SemaphoreSlim.WaitAsync();

            Stopwatch stopwatch = StopWatchStart();

            IEnumerable<C> result;

            try
            {
                using IDbConnection connection = SqlWriter.Config.CreateDbConnection();

                result = await RegisteredClass.QueryAsync(sql, splitOn, P, connection, CommandTimeOut);

                StopWatchEnd(stopwatch, sql);

                PrepareResults(queryType, result);

                return result;
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                SqlWriter.OnQueryFailure(this, new QueryFailure(nameof(QueryAsync), e, sql, stopwatch));

                throw error;
            }
            finally
            {
                if (SqlWriter.Config.AllowConcurrentQueries == false)
                    SqlWriter.SemaphoreSlim.Release();
            }
        }

        protected async Task<C> QueryFirstAsync(QueryType queryType, string sql)
        {
            if (SqlWriter.Config.AllowConcurrentQueries == false)
                await SqlWriter.SemaphoreSlim.WaitAsync();

            Stopwatch stopwatch = StopWatchStart();

            C result;
            
            try
            {
                using IDbConnection connection = SqlWriter.Config.CreateDbConnection();

                result = await connection.QueryFirstAsync<C>(sql, P, null, CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                PrepareResult(queryType, result);

                return result;
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                SqlWriter.OnQueryFailure(this, new QueryFailure(nameof(QueryFirstAsync), e, sql, stopwatch));

                throw error;
            }
            finally
            {
                if (SqlWriter.Config.AllowConcurrentQueries == false)
                    SqlWriter.SemaphoreSlim.Release();
            }
        }

        protected async Task<C?> QueryFirstOrDefaultAsync(QueryType queryType, string sql)
        {
            if (SqlWriter.Config.AllowConcurrentQueries == false)
                await SqlWriter.SemaphoreSlim.WaitAsync();

            Stopwatch stopwatch = StopWatchStart();

            C? result;

            try
            {
                using IDbConnection connection = SqlWriter.Config.CreateDbConnection();
                
                result = await connection.QueryFirstOrDefaultAsync<C>(sql, P, null, CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                PrepareResult(queryType, result);

                return result;
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                SqlWriter.OnQueryFailure(this, new QueryFailure(nameof(QueryFirstOrDefaultAsync), e, sql, stopwatch));

                throw error;
            }
            finally
            {
                if (SqlWriter.Config.AllowConcurrentQueries == false)
                    SqlWriter.SemaphoreSlim.Release();
            }
        }

        protected async Task<C> QuerySingleAsync(QueryType queryType, string sql)
        {
            if (SqlWriter.Config.AllowConcurrentQueries == false)
                await SqlWriter.SemaphoreSlim.WaitAsync();

            Stopwatch stopwatch = StopWatchStart();

            C result;

            try
            {
                using IDbConnection connection = SqlWriter.Config.CreateDbConnection();

                result = await connection.QuerySingleAsync<C>(sql, P, null, CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                PrepareResult(queryType, result);

                return result;
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                SqlWriter.OnQueryFailure(this, new QueryFailure(nameof(QuerySingleAsync), e, sql, stopwatch));

                throw error;
            }
            finally
            {
                if (SqlWriter.Config.AllowConcurrentQueries == false)
                    SqlWriter.SemaphoreSlim.Release();
            }
        }

        protected async Task<C?> QuerySingleOrDefaultAsync(QueryType queryType, string sql)
        {
            if (SqlWriter.Config.AllowConcurrentQueries == false)
                await SqlWriter.SemaphoreSlim.WaitAsync();

            Stopwatch stopwatch = StopWatchStart();

            C? result;

            try
            {
                using IDbConnection connection = SqlWriter.Config.CreateDbConnection();

                result = await connection.QuerySingleOrDefaultAsync<C>(sql, P, null, CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);

                PrepareResult(queryType, result);

                return result;
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                SqlWriter.OnQueryFailure(this, new QueryFailure(nameof(QuerySingleOrDefaultAsync), e, sql, stopwatch));

                throw error;
            }
            finally
            {
                if (SqlWriter.Config.AllowConcurrentQueries == false)
                    SqlWriter.SemaphoreSlim.Release();
            }
        }

        protected async Task ExecuteAsync(string sql)
        {
            if (SqlWriter.Config.AllowConcurrentQueries == false)
                await SqlWriter.SemaphoreSlim.WaitAsync();

            Stopwatch stopwatch = StopWatchStart();

            try
            {
                using IDbConnection connection = SqlWriter.Config.CreateDbConnection();

                await connection.ExecuteAsync(sql, P, null, CommandTimeOut).ConfigureAwait(false);

                StopWatchEnd(stopwatch, sql);
            }
            catch (Exception e)
            {
                StopWatchEnd(stopwatch, sql);

                var error = GetException(e);

                SqlWriter.OnQueryFailure(this, new QueryFailure(nameof(ExecuteAsync), e, sql, stopwatch));

                throw error;
            }
            finally
            {
                if (SqlWriter.Config.AllowConcurrentQueries == false)
                    SqlWriter.SemaphoreSlim.Release();
            }
        }

        public int? CommandTimeOut { get; set; }

        private SqlWriterException GetException(Exception e) => new SqlWriterException("Error executing query.", e);
    }
}
