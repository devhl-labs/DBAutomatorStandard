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

using static devhl.DBAutomator.PostgresMethods;
using devhl.DBAutomator.Interfaces;

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

        public async Task<IEnumerable<C>> UpdateAsync(Expression<Func<C, object>> setCollection, Expression<Func<C, object>>? whereCollection = null)
        {         
            RegisteredClass registeredClass = _dBAutomator.RegisteredClasses.First(r => r.SomeClass.GetType() == typeof(C));

            List<ExpressionModel<C>> setExpressions = new List<ExpressionModel<C>>();

            BinaryExpression? setBinaryExpression = GetBinaryExpression(setCollection);

            GetExpressions(setBinaryExpression, setExpressions, registeredClass, "s_");

            List<ExpressionModel<C>> whereExpressions = new List<ExpressionModel<C>>();

            BinaryExpression? whereBinaryExpression = GetBinaryExpression(whereCollection);

            GetExpressions(whereBinaryExpression, whereExpressions, registeredClass);

            string sql = $"UPDATE \"{registeredClass.TableName}\" SET {setCollection.GetWhereClause(setExpressions)}";

            if (whereCollection != null)
            {
                sql = $"{sql} WHERE {whereCollection.GetWhereClause(whereExpressions)}";
            }

            sql = $"{sql} RETURNING *;";

            _logger.LogTrace(sql);

            DynamicParameters p = GetDynamicParametersFromExpression(setExpressions);

            GetDynamicParametersFromExpression(whereExpressions, p);

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
            if (item == null)
            {
                throw new NullReferenceException("The item cannot be null.");
            }

            RegisteredClass registeredClass = _dBAutomator.RegisteredClasses.First(r => r.SomeClass.GetType() == typeof(C));

            DynamicParameters p = GetDynamicParameters(item, registeredClass.RegisteredProperties);

            var keys = registeredClass.RegisteredProperties.Where(p => p.IsKey);

            if (keys.Count() == 0)
            {
                throw new Exception("The registered class does not have a primary key attribute.");
            }

            string sql = $"UPDATE \"{registeredClass.TableName}\" SET {GetWhereClause(item, registeredClass.RegisteredProperties.Where(p => !p.IsAutoIncrement).ToList(), ",")} WHERE";

            foreach(var key in keys)
            {
                sql = $"{sql} \"{key.ColumnName}\" = '";

                if (key.PropertyType == typeof(ulong) || key.PropertyType == typeof(ulong?))
                {
                    sql = $"{sql}{Convert.ToInt64(item.GetType().GetProperty(key.PropertyName).GetValue(item, null))}";
                }
                else
                {
                    sql = $"{sql}{item.GetType().GetProperty(key.PropertyName).GetValue(item, null)}";
                }

                sql = $"{sql}' AND";
            }

            sql = sql[0..^3];

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
