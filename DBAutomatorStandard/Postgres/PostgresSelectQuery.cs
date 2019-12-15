using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Dapper;
using Npgsql;

using static devhl.DBAutomator.PostgresMethods;

namespace devhl.DBAutomator
{
    internal class PostgresSelectQuery <C> : ISelectQuery<C>
    {
        private readonly DBAutomator _dBAutomator;
        private readonly QueryOptions _queryOptions;
        private readonly ILogger? _logger;

        public PostgresSelectQuery(DBAutomator dBAutomator, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;
            _queryOptions = queryOptions;
            _logger = logger;
        }

        public async Task<IEnumerable<C>> GetAsync(Expression<Func<C, object>>? where = null, OrderByClause<C>? orderBy = null)
        {
            RegisteredClass registeredClass = _dBAutomator.RegisteredClasses.First(r => r.SomeClass.GetType() == typeof(C));

            List<ExpressionModel<C>> expressions = new List<ExpressionModel<C>>();

            BinaryExpression? binaryExpression = GetBinaryExpression(where);

            GetExpressions(binaryExpression, expressions, registeredClass);

            string sql = $"SELECT";

            foreach(var property in registeredClass.RegisteredProperties)
            {
                sql = $"{sql} \"{property.ColumnName}\",";
            }

            sql = sql[0..^1];

            sql = $"{sql} FROM \"{registeredClass.TableName}\"";

            if (where != null)
            {
                sql = $"{sql} WHERE {where.GetWhereClause(expressions)}";
            }

            if (orderBy != null)
            {
                sql = $"{sql} {orderBy.GetOrderByClause()}";
            }

            sql = $"{sql};";

            _logger.LogTrace(sql);

            DynamicParameters p = GetDynamicParametersFromExpression(expressions);

            using NpgsqlConnection connection = new NpgsqlConnection(_queryOptions.ConnectionString);

            await connection.OpenAsync();

            Stopwatch stopwatch = StopWatchStart();

            var result = await connection.QueryAsync<C>(sql, p, _queryOptions.DbTransaction, _queryOptions.CommandTimeOut);

            StopWatchEnd(stopwatch, "GetAsync()");

            foreach(var item in result)
            {
                if (item is IDBObject dBObject)
                {
                    await dBObject.OnLoadedAsync(_dBAutomator);
                }
            }

            return result;

            //try
            //{
            //    C result = await connection.QueryFirstOrDefaultAsync<C>(StoredProcedureName, _dynamicParameters, _dbTransaction, _commandTimeout, CommandType.StoredProcedure);

            //    await OnLoaded(result, _dBAutomator);

            //    return result;
            //}
            //finally
            //{
            //    StopWatchEnd(stopwatch, "GetAsync()");

            //    connection.Close();
            //}
        }

        //public async Task<C> GetFirstOrDefaultAsync(Expression<Func<C, object>>? where = null, OrderByClause<C>? orderBy = null)
        //{
        //    var result = await GetAsync(where, orderBy);

        //    return result.FirstOrDefault();
        //}

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
