using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static DBAutomatorStandard.Statics;

namespace DBAutomatorStandard
{
    internal class PostgresDeleteQuery <C> : IDeleteQuery<C>
    {
        private readonly DBAutomator _dBAutomator;
        private readonly QueryOptions _queryOptions;
        private readonly ILogger? _logger;

        public PostgresDeleteQuery(DBAutomator dBAutomator, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;
            _queryOptions = queryOptions;
            _logger = logger;
        }

        public async Task<int> DeleteAsync(C item)
        {
            if (item == null)
            {
                throw new NullReferenceException("The item cannot be null.");
            }

            DynamicParameters p = new DynamicParameters();

            RegisteredClass registeredClass = _dBAutomator.RegisteredClasses.First(r => r.SomeClass.GetType() == typeof(C));

            string sql = $"DELETE FROM \"{registeredClass.TableName}\" WHERE {GetWhereClause(item, registeredClass)};";

            _logger.LogTrace(sql);

            using NpgsqlConnection connection = new NpgsqlConnection(_queryOptions.ConnectionString);

            await connection.OpenAsync();

            Stopwatch stopwatch = StopWatchStart();

            var result = await connection.ExecuteAsync(sql, p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut);

            StopWatchEnd(stopwatch, "GetAsync()");

            return result;
        }

        public async Task<IEnumerable<C>> DeleteAsync(Expression<Func<C, object>>? where = null)
        {
            //todo is item an ienumerable?

            DynamicParameters p = new DynamicParameters();

            RegisteredClass registeredClass = _dBAutomator.RegisteredClasses.First(r => r.SomeClass.GetType() == typeof(C));

            string sql = $"DELETE FROM \"{registeredClass.TableName}\" WHERE {where.GetWhereClause(registeredClass)} RETURNING *;";

            _logger.LogTrace(sql);

            using NpgsqlConnection connection = new NpgsqlConnection(_queryOptions.ConnectionString);

            await connection.OpenAsync();

            Stopwatch stopwatch = StopWatchStart();

            var result = await connection.QueryAsync<C>(sql, p);

            StopWatchEnd(stopwatch, "GetAsync()");

            return result;
        }

        private Stopwatch StopWatchStart()
        {
            Stopwatch result = new Stopwatch();
            result.Start();
            return result;
        }

        private void StopWatchEnd(Stopwatch stopwatch, string methodName)
        {
            stopwatch.Stop();
            if (stopwatch.Elapsed > _queryOptions.SlowQueryWarning)
            {
                _dBAutomator.SlowQueryDetected(methodName, stopwatch.Elapsed);
            }
        }
    }
}
