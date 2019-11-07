using Dapper;
using DBAutomatorStandard;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static DBAutomatorLibrary.Statics;
using static DBAutomatorStandard.Enums;

namespace DBAutomatorLibrary
{
    internal class PostgresSelectQuery<I, C> : ISelectQuery<I, C> where C : I
    {
        private const string _source = nameof(PostgresSelectQuery<I, C>);
        private readonly string _connectionString;
        private readonly IDbTransaction? _dbTransaction;
        private readonly int? _commandTimeout;
        private readonly DBAutomator _dBAutomator;
        private readonly int _slowQueryWarningInSeconds;
        private readonly DynamicParameters _dynamicParameters = new DynamicParameters();
        private readonly string _schema;


        public Expression<Func<C, object>>? Collection { get; }
        public List<ConditionModel>? ConditionModels { get; }
        public string TableName { get; }
        public string StoredProcedureName { get; }





        public PostgresSelectQuery(DBAutomator dBAutomator, string connectionString, int slowQueryWarningInSeconds, Expression<Func<C, object>>? collection, IDbTransaction? dbTransaction = null, int? commandTimeout = null, string schema = "public")
        {
            _dBAutomator = dBAutomator;
            _connectionString = connectionString;
            _slowQueryWarningInSeconds = slowQueryWarningInSeconds;
            _dbTransaction = dbTransaction;
            _commandTimeout = commandTimeout;
            _schema = schema;

            Collection = collection;
            ConditionModels = collection.GetConditions();
            TableName = PostgresMapping.GetTableName<I>();
            StoredProcedureName = PostgresMapping.GetProcedureName<I>(QueryType.Get, TableName, ConditionModels);

            foreach (ConditionModel condition in ConditionModels ?? Enumerable.Empty<ConditionModel>())
            {
                PostgresMapping.AddParameter(_dynamicParameters, condition);
            }


        }

        public async Task<List<I>> GetListAsync()
        {
            using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            Stopwatch stopwatch = StopWatchStart();

            try
            {
                IEnumerable<C> obj;

                if (_dynamicParameters.ParameterNames.Count() == 0)
                {
                    obj = await connection.QueryAsync<C>(StoredProcedureName, null, _dbTransaction, _commandTimeout, CommandType.StoredProcedure);

                }
                else
                {
                    obj = await connection.QueryAsync<C>(StoredProcedureName, _dynamicParameters, _dbTransaction, _commandTimeout, CommandType.StoredProcedure);
                }

                var result = obj.Cast<I>().ToList();

                foreach (I item in result)
                {
                    if (item is IDBObject dBObject)
                    {
                        await dBObject.OnLoaded(_dBAutomator);
                    }

                }

                return result;
            }
            catch (PostgresException e)
            {
                if (e.SqlState == "42883")  //function does not exist
                {
                    CreateGet(connection, ConditionModels);

                    IEnumerable<C> obj = await connection.QueryAsync<C>(StoredProcedureName, _dynamicParameters, _dbTransaction, _commandTimeout, CommandType.StoredProcedure);

                    var result = obj.Cast<I>().ToList();

                    foreach (I item in result)
                    {
                        if (item is IDBObject dBObject)
                        {
                            await dBObject.OnLoaded(_dBAutomator);
                        }

                    }

                    return result;
                }
                
                _dBAutomator.Logger?.LogWarning(LoggingEvents.ErrorExecutingQuery, "{source}: {method} {message}", _source, "GetListAsync", e.Message);

                throw;
            }

            finally
            {
                StopWatchEnd(stopwatch, "GetAsync()");

                connection.Close();
            }
        }

        public async Task<I> GetAsync()
        {
            using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            Stopwatch stopwatch = StopWatchStart();

            try
            {
                C result = await connection.QueryFirstOrDefaultAsync<C>(StoredProcedureName, _dynamicParameters, _dbTransaction, _commandTimeout, CommandType.StoredProcedure);

                await OnLoaded(result, _dBAutomator);

                return result;
            }
            catch (PostgresException e)
            {
                if (e.SqlState == "42883")  //function does not exist
                {
                    CreateGet(connection, ConditionModels);

                    C result = await connection.QueryFirstOrDefaultAsync<C>(StoredProcedureName, _dynamicParameters, _dbTransaction, _commandTimeout, CommandType.StoredProcedure);

                    await OnLoaded(result, _dBAutomator);

                    return result;
                }

                _dBAutomator.Logger?.LogWarning(LoggingEvents.ErrorExecutingQuery, "{source}: {method} {message}", _source, "GetAsync", e.Message);

                throw;
            }

            finally
            {
                StopWatchEnd(stopwatch, "GetAsync()");

                connection.Close();
            }
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
            if (stopwatch.Elapsed.TotalSeconds > _slowQueryWarningInSeconds)
            {
                _dBAutomator.SlowQueryDetected(methodName, stopwatch.Elapsed);
            }
        }














        private void CreateGet(NpgsqlConnection connection, List<ConditionModel>? conditions)
        {
            _dBAutomator.Logger?.LogDebug(LoggingEvents.ModifyingDatabase, "{source}: {method}", _source, "CreateGet");

            using NpgsqlCommand command = new NpgsqlCommand
            {
                Connection = connection
            };

            string commandText = "";

            commandText = $"{commandText}CREATE OR REPLACE FUNCTION {_schema}.{StoredProcedureName}(\n";

            foreach (ConditionModel condition in conditions ?? Enumerable.Empty<ConditionModel>())
            {
                if (condition.Value == null)
                {
                    throw new ArgumentNullException();
                }

                commandText = $"{commandText}_{condition.Name.ToLower()} {condition.Value.MapToPostgreSql()}\n, ";
            }

            if (commandText.Right(2) == ", ")
            {
                commandText = commandText.Left(commandText.Length - 2);
            }


            commandText = $"{commandText})\n";

            commandText = $"{commandText}RETURNS TABLE(\n";

            foreach (PropertyInfo property in typeof(I).GetProperties())
            {
                if (property.IsStorable())
                {
                    commandText = $"{commandText}{property.Name} {property.MapToPostgreSql()}\n, ";
                }
            }

            commandText = commandText.Substring(0, commandText.Length - 2);

            commandText = $"{commandText})\n";

            commandText = $"{commandText}LANGUAGE SQL\n";

            commandText = $"{commandText}AS $$\n\n";

            //commandText = $"{commandText}BEGIN\n";

            //commandText = $"{commandText}RETURN QUERY \n";

            commandText = $"{commandText}SELECT \n";

            foreach (PropertyInfo property in typeof(I).GetProperties())
            {
                if (property.IsStorable())
                {
                    commandText = $"{commandText}\"{TableName}\".\"{PostgresMapping.GetColumnName<I>(property.Name)}\"\n, ";  //property.Name
                }
            }
            commandText = commandText.Substring(0, commandText.Length - 2);

            commandText = $"{commandText}\n";
            commandText = $"{commandText}FROM {_schema}.\"{TableName}\"\n";

            if (conditions != null && conditions.Count() > 0)
            {
                commandText = $"{commandText}WHERE ";
                foreach (ConditionModel condition in conditions)
                {
                    commandText = $"{commandText}\"{TableName}\".\"{PostgresMapping.GetColumnName<I>(condition.Name)}\" {condition.Operator} _{condition.Name} AND ";
                }
                commandText = commandText.Substring(0, commandText.Length - 5);
            }

            commandText = $"{commandText};\n\n";
            //commandText = $"{commandText}END; \n\n";
            commandText = $"{commandText}$$\n";

            command.CommandText = commandText;
            command.ExecuteNonQuery();

        }
    }
}
