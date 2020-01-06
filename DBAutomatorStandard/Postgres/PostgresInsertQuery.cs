using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Dapper;
using Npgsql;

using static devhl.DBAutomator.PostgresMethods;
using devhl.DBAutomator.Interfaces;

namespace devhl.DBAutomator
{
    internal class PostgresInsertQuery <C> : IInsertQuery <C>
    {
        private readonly DBAutomator _dBAutomator;
        private readonly QueryOptions _queryOptions;
        private readonly ILogger? _logger;

        public PostgresInsertQuery(DBAutomator dBAutomator, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;
            _queryOptions = queryOptions;
            _logger = logger;
        }

        public async Task<C> InsertAsync(C item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "The item cannot be null");
            }

            RegisteredClass<C> registeredClass = (RegisteredClass<C>) _dBAutomator.RegisteredClasses.First(r => r is RegisteredClass<C>);

            DynamicParameters p = GetDynamicParameters(item, registeredClass.RegisteredProperties);

            string sql = $"INSERT INTO \"{registeredClass.TableName}\" (";

            foreach (var property in registeredClass.RegisteredProperties.Where(p => !p.IsAutoIncrement))
            {
                sql = $"{sql}\"{property.ColumnName}\", ";
            }

            sql = sql[0..^2];

            sql = $"{sql}) VALUES (";

            foreach (var property in registeredClass.RegisteredProperties.Where(p => !p.IsAutoIncrement))
            {
                sql = $"{sql}@{property.ColumnName}, ";
            }

            sql = sql[0..^2];

            sql = $"{sql}) RETURNING *;";

            _logger.LogTrace(sql);

            if (item is IDBEvent dBObject)
            {
                await dBObject.OnInsertAsync(_dBAutomator).ConfigureAwait(false);
            }

            using NpgsqlConnection connection = new NpgsqlConnection(_queryOptions.ConnectionString);

            connection.Open();

            var stopWatch = StopWatchStart();

            var result = await connection.QueryFirstOrDefaultAsync<C>(sql, p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut).ConfigureAwait(false);

            StopWatchEnd(stopWatch, "PostgresInsertQuery");

            connection.Close();

            if (item is IDBObject onInsertedObject)
            {
                onInsertedObject.IsDirty = false;

                onInsertedObject.IsNew = false;
            }

            if (item is IDBEvent onInsertedEvent)
            {
                await onInsertedEvent.OnInsertedAsync(_dBAutomator).ConfigureAwait(false);
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
