using System;
using System.Collections.Generic;
using System.Data;
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
    internal class PostgresUpdateQuery<C> : IUpdateQuery<C>
    {
        private readonly DBAutomator _dBAutomator;

        private readonly QueryOptions _queryOptions;

        private readonly ILogger? _logger;

        public PostgresUpdateQuery(DBAutomator dBAutomator, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;
            _queryOptions = queryOptions;
            _logger = logger;
        }

        public async Task<IEnumerable<C>> UpdateAsync(Expression<Func<C, object>> set, Expression<Func<C, object>>? where = null)
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) _dBAutomator.RegisteredClasses.First(r => r is RegisteredClass<C>);

            BinaryExpression? whereBinary = PostgresMethods.GetBinaryExpression(where);

            List<ExpressionPart>? whereExpressionParts = PostgresMethods.GetExpressionParts(whereBinary);

            BinaryExpression? setBinary = PostgresMethods.GetBinaryExpression(set);

            List<ExpressionPart>? setExpressionParts = PostgresMethods.GetExpressionParts(setBinary);

            if (setExpressionParts == null) throw new DbAutomatorException("Unsupported expression", new ArgumentException());

            string sql = $"UPDATE \"{registeredClass.TableName}\" SET {PostgresMethods.ToColumnNameEqualsParameterName(setExpressionParts, "s_")}";

            if (whereExpressionParts != null)
            {
                sql = $"{sql} WHERE {PostgresMethods.ToColumnNameEqualsParameterName(whereExpressionParts, "w_")}";
            }

            sql = $"{sql} RETURNING *;";

            _logger.LogTrace(sql);

            DynamicParameters p = new DynamicParameters();

            PostgresMethods.AddParameters(p, registeredClass, whereExpressionParts);

            PostgresMethods.AddParameters(p, registeredClass, setExpressionParts, "s_");

            using NpgsqlConnection connection = new NpgsqlConnection(_queryOptions.ConnectionString);

            connection.Open();

            var stopWatch = StopWatchStart();

            var result = await connection.QueryAsync<C>(sql, p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

            StopWatchEnd(stopWatch, "UpdateAsync");

            connection.Close();

            return result;
        }

        public async Task<C> UpdateAsync(C item)
        {
            if (item == null) throw new NullReferenceException("The item cannot be null.");

            RegisteredClass<C> registeredClass = (RegisteredClass<C>) _dBAutomator.RegisteredClasses.First(r => r is RegisteredClass<C>);

            if (registeredClass.RegisteredProperties.Count(p => !p.NotMapped && p.IsKey) == 0) throw new Exception("The registered class does not have a primary key attribute.");

            string sql = $"UPDATE \"{registeredClass.TableName}\" SET {PostgresMethods.ToColumnNameEqualsParameterName(registeredClass.RegisteredProperties.Where(p => !p.NotMapped), "s_")} WHERE ";

            sql = $"{sql} {PostgresMethods.ToColumnNameEqualsParameterName(registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey))} ";

            DynamicParameters p = new DynamicParameters();

            PostgresMethods.AddParameters(p, item, registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement), "s_");

            PostgresMethods.AddParameters(p, item, registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement && p.IsKey));

            sql = $"{sql} RETURNING *;";

            _logger.LogTrace(sql);

            if (item is IDBEvent dBObject)
            {
                await dBObject.OnUpdateAsync(_dBAutomator).ConfigureAwait(false);
            }

            using NpgsqlConnection connection = new NpgsqlConnection(_queryOptions.ConnectionString);

            connection.Open();

            var stopWatch = StopWatchStart();

            var result = await connection.QuerySingleAsync<C>(sql, p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

            StopWatchEnd(stopWatch, "UpdateAsync");

            connection.Close();

            if (item is IDBObject dbObjectUpdated)
            {
                dbObjectUpdated.IsNew = false;

                dbObjectUpdated.IsDirty = false;
            }

            if (item is IDBEvent onUpdatedEvent)
            {
                await onUpdatedEvent.OnUpdatedAsync(_dBAutomator).ConfigureAwait(false);
            }

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
