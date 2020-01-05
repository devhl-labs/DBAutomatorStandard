using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using static devhl.DBAutomator.PostgresMethods;

using Dapper;
using Npgsql;
using devhl.DBAutomator.Interfaces;

namespace devhl.DBAutomator
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

            RegisteredClass registeredClass = _dBAutomator.RegisteredClasses.First(r => r.SomeClass.GetType() == typeof(C));

            DynamicParameters p = GetDynamicParameters(item, registeredClass.RegisteredProperties);

            string sql = $"DELETE FROM \"{registeredClass.TableName}\" WHERE {GetWhereClause(item, registeredClass.RegisteredProperties)};";

            _logger.LogTrace(sql);

            if (item is IDBEvent dBObject)
            {
                await dBObject.OnDeleteAsync(_dBAutomator).ConfigureAwait(false);
            }

            using NpgsqlConnection connection = new NpgsqlConnection(_queryOptions.ConnectionString);

            await connection.OpenAsync().ConfigureAwait(false);

            Stopwatch stopwatch = StopWatchStart();

            var result = await connection.ExecuteAsync(sql, p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

            StopWatchEnd(stopwatch, "GetAsync()");

            if (item is IDBEvent dBObject1)
            {
                await dBObject1.OnDeletedAsync(_dBAutomator).ConfigureAwait(false);
            }

            return result;
        }

        public async Task<IEnumerable<C>> DeleteAsync(Expression<Func<C, object>>? where = null)
        {
            RegisteredClass registeredClass = _dBAutomator.RegisteredClasses.First(r => r.SomeClass.GetType() == typeof(C));

            List<ExpressionModel<C>> expressions = new List<ExpressionModel<C>>();

            BinaryExpression? binaryExpression = GetBinaryExpression(where);

            GetExpressions(binaryExpression, expressions, registeredClass);

            string sql = $"DELETE FROM \"{registeredClass.TableName}\"";

            if (where != null)
            {
                sql = $"{sql} WHERE {where.GetWhereClause(expressions)}";
            }

            sql = $"{sql} RETURNING *;";

            _logger.LogTrace(sql);

            DynamicParameters p = GetDynamicParametersFromExpression(expressions);

            using NpgsqlConnection connection = new NpgsqlConnection(_queryOptions.ConnectionString);

            await connection.OpenAsync().ConfigureAwait(false);

            Stopwatch stopwatch = StopWatchStart();

            var result = await connection.QueryAsync<C>(sql, p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

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
