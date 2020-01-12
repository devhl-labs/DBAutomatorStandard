﻿using Dapper;
using devhl.DBAutomator.Interfaces;
using devhl.DBAutomator.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace devhl.DBAutomator
{
    public abstract class BasePostgresQuery<C>
    {
#nullable disable

        protected DBAutomator _dBAutomator;

        protected QueryOptions _queryOptions;

        protected RegisteredClass<C> _registeredClass;

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

            if (stopwatch.Elapsed > _queryOptions.SlowQueryWarning) _dBAutomator.SlowQueryDetected(methodName, stopwatch.Elapsed);
        }

        protected async Task PrepareResult(IEnumerable<C> results)
        {
            foreach (var result in results) await PrepareResult(result).ConfigureAwait(false);
        }

        protected async Task PrepareResult(C result)
        {
            if (result is IDBObject dbObjectLoaded)
            {
                dbObjectLoaded.IsNew = false;

                dbObjectLoaded.IsDirty = false;
            }

            if (result is IDBEvent dbEventLoaded)
            {
                await dbEventLoaded.OnLoadedAsync(_dBAutomator).ConfigureAwait(false);
            }
        }

        protected async Task<NpgsqlConnection> OpenConnection()
        {
            NpgsqlConnection connection = new NpgsqlConnection(_queryOptions.ConnectionString);

            await connection.OpenAsync().ConfigureAwait(false);

            return connection;
        }

        protected async Task<IEnumerable<C>> QueryAsync(string sql)
        {
            using NpgsqlConnection connection = await OpenConnection().ConfigureAwait(false);

            Stopwatch stopwatch = StopWatchStart();

            IEnumerable<C> result;

            try
            {
                result = await connection.QueryAsync<C>(sql, _p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

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

            await PrepareResult(result).ConfigureAwait(false);

            return result;
        }

        protected async Task<C> QueryFirstAsync(string sql)
        {
            using NpgsqlConnection connection = await OpenConnection().ConfigureAwait(false);

            Stopwatch stopwatch = StopWatchStart();

            C result;

            try
            {
                result = await connection.QueryFirstAsync<C>(sql, _p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

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

            await PrepareResult(result).ConfigureAwait(false);

            return result;
        }

        protected async Task<C> QueryFirstOrDefaultAsync(string sql)
        {
            using NpgsqlConnection connection = await OpenConnection().ConfigureAwait(false);

            Stopwatch stopwatch = StopWatchStart();

            C result;

            try
            {
                result = await connection.QueryFirstOrDefaultAsync<C>(sql, _p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

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

            await PrepareResult(result).ConfigureAwait(false);

            return result;
        }

        protected async Task<C> QuerySingleAsync(string sql)
        {
            using NpgsqlConnection connection = await OpenConnection().ConfigureAwait(false);

            Stopwatch stopwatch = StopWatchStart();

            C result;

            try
            {
                result = await connection.QuerySingleAsync<C>(sql, _p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

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

            await PrepareResult(result).ConfigureAwait(false);

            return result;
        }

        protected async Task<C> QuerySingleOrDefaultAsync(string sql)
        {
            using NpgsqlConnection connection = await OpenConnection().ConfigureAwait(false);

            Stopwatch stopwatch = StopWatchStart();

            C result;

            try
            {
                result = await connection.QuerySingleOrDefaultAsync<C>(sql, _p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

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

            await PrepareResult(result).ConfigureAwait(false);

            return result;
        }

        private DbAutomatorException GetException(Exception e) => new DbAutomatorException("Error executing query.", e);
    }
}
