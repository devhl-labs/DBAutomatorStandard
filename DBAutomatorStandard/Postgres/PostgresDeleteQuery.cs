using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Dapper;
using Npgsql;
using devhl.DBAutomator.Interfaces;
using devhl.DBAutomator.Models;

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
            if (item == null) throw new NullReferenceException("The item cannot be null.");

            RegisteredClass<C> registeredClass = (RegisteredClass<C>) _dBAutomator.RegisteredClasses.First(r => r is RegisteredClass<C>);

            DynamicParameters p = new DynamicParameters();

            string sql = $"DELETE FROM \"{registeredClass.TableName}\" WHERE ";

            if (registeredClass.RegisteredProperties.Any(p => p.IsKey))
            {
                sql = $"{sql}{PostgresMethods.ToColumnNameEqualsParameterName(registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey))}";

                PostgresMethods.AddParameters(p, item, registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement && p.IsKey));
            }
            else
            {
                sql = $"{sql}{PostgresMethods.ToColumnNameEqualsParameterName(registeredClass.RegisteredProperties.Where(p => !p.NotMapped))}";

                PostgresMethods.AddParameters(p, item, registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement));
            }

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
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) _dBAutomator.RegisteredClasses.First(r => r is RegisteredClass<C>);

            BinaryExpression? binaryExpression = PostgresMethods.GetBinaryExpression(where);

            List<ExpressionPart>? expressionParts = PostgresMethods.GetExpressionParts(binaryExpression);

            DynamicParameters p = new DynamicParameters();

            string sql = $"DELETE FROM \"{registeredClass.TableName}\" ";

            if (where != null)
            {
                if (expressionParts == null) throw new DbAutomatorException("Unsupported expression", new ArgumentException());

                sql = $"{sql}WHERE ";

                sql = $"{sql}{PostgresMethods.ToColumnNameEqualsParameterName(expressionParts)} ";

                PostgresMethods.AddParameters(p, registeredClass, expressionParts);
            }

            sql = $"{sql} RETURNING *;";

            _logger.LogTrace(sql);

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
