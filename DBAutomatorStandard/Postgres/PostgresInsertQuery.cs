using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static DBAutomatorStandard.Statics;

namespace DBAutomatorStandard
{
    internal class PostgresInsertQuery : IInsertQuery
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

        public async Task<int> InsertAsync(object item)
        {
            DynamicParameters p = new DynamicParameters();

            RegisteredClass registeredClass = _dBAutomator.RegisteredClasses.First(r => r.SomeClass.GetType() == item.GetType());

            string sql = $"INSERT INTO \"{registeredClass.TableName}\" (";

            foreach (var property in registeredClass.RegisteredProperties)
            {
                sql = $"{sql}\"{property.ColumnName}\", ";

                //if (property.PropertyType == typeof(ulong))
                //{
                //    p.Add(property.ColumnName, Convert.ToInt64(item.GetType().GetProperty(property.PropertyName).GetValue(item)));
                //}
                //else
                //{
                //    p.Add(property.ColumnName, item.GetType().GetProperty(property.PropertyName).GetValue(item));
                //}

                p = GetDynamicParameters(item, registeredClass);
            }

            sql = sql[0..^2];

            sql = $"{sql}) VALUES (";

            foreach (var property in registeredClass.RegisteredProperties)
            {
                sql = $"{sql}@{property.ColumnName}, ";
            }

            sql = sql[0..^2];

            sql = $"{sql});";

            _logger.LogTrace(sql);

            if (item is IDBObject dBObject)
            {
                await dBObject.OnInsertAsync(_dBAutomator);
            }

            using NpgsqlConnection connection = new NpgsqlConnection(_queryOptions.ConnectionString);

            connection.Open();

            var stopWatch = StopWatchStart();

            int result = await connection.ExecuteAsync(sql, p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut);

            StopWatchEnd(stopWatch, "PostgresInsertQuery");

            connection.Close();

            if (item is IDBObject dBObject1)
            {
                await dBObject1.OnInsertAsync(_dBAutomator);
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
