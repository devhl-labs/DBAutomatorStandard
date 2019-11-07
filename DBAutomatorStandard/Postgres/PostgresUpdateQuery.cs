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
    internal class PostgresUpdateQuery<I, C> : IUpdateQuery<I, C> where C : I where I : class
    {
        private const string _source = nameof(PostgresUpdateQuery<I, C>);
        private readonly string _connectionString;
        private readonly IDbTransaction? _dbTransaction;
        private readonly int? _commandTimeout;
        private readonly DBAutomator _dBAutomator;
        private readonly int _slowQueryWarningInSeconds;
        private readonly DynamicParameters _dynamicParameters = new DynamicParameters();
        //private readonly string _schema;
        private readonly I? _item;

        public string TableName { get; }
        public string StoredProcedureName { get; }
        public List<ConditionModel> WhereConditionModels { get; } = new List<ConditionModel>();
        public List<ConditionModel> SetConditionModels { get; } = new List<ConditionModel>();



        public PostgresUpdateQuery(DBAutomator dBAutomator, string connectionString, int slowQueryWarningInSeconds, Expression<Func<C, object>> setCollection, Expression<Func<C, object>>? whereCollection = null, IDbTransaction? dbTransaction = null, int? commandTimeout = null)
        {
            _dBAutomator = dBAutomator;
            _connectionString = connectionString;
            _slowQueryWarningInSeconds = slowQueryWarningInSeconds;
            _dbTransaction = dbTransaction;
            _commandTimeout = commandTimeout;
            //_schema = schema;

            WhereConditionModels = whereCollection.GetConditions();
            SetConditionModels = setCollection.GetConditions();
            TableName = PostgresMapping.GetTableName<I>();
            StoredProcedureName = PostgresMapping.GetProcedureName<I>(QueryType.Update, TableName, WhereConditionModels, SetConditionModels);

            foreach (ConditionModel condition in WhereConditionModels ?? Enumerable.Empty<ConditionModel>())
            {
                PostgresMapping.AddParameter(_dynamicParameters, condition, "_w");
            }

            foreach (ConditionModel condition in SetConditionModels)
            {
                PostgresMapping.AddParameter(_dynamicParameters, condition, "_s");
            }
        }

        public PostgresUpdateQuery(I item, DBAutomator dBAutomator, string connectionString, int slowQueryWarningInSeconds, IDbTransaction? dbTransaction = null, int? commandTimeout = null)
        {
            List<PropertyInfo> props = typeof(I).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(IdentityAttribute))).ToList();

            if (props.Count() == 0)
            {
                throw new Exception($"DBAutomator Error: The item {typeof(I).Name} does not contain an identity attribute.");
            }

            List<ConditionModel> conditionModels = new List<ConditionModel>();

            _dBAutomator = dBAutomator;
            _connectionString = connectionString;
            _slowQueryWarningInSeconds = slowQueryWarningInSeconds;
            _dbTransaction = dbTransaction;
            _commandTimeout = commandTimeout;
            //_schema = schema;
            _item = item;

            foreach (var prop in props)
            {
                ConditionModel conditionModel = new ConditionModel
                {
                    Name = PostgresMapping.GetColumnName<I>(prop.Name),
                    OperatorName = "Equal",
                    Value = prop.GetValue(item)
                };

                WhereConditionModels.Add(conditionModel);
            }

            foreach (PropertyInfo prop in typeof(I).GetProperties())
            {
                if (!prop.IsStorable()) { continue; }

                ConditionModel conditionModel = new ConditionModel
                {
                    Name = PostgresMapping.GetColumnName<I>(prop.Name),
                    OperatorName = "Equal",
                    Value = prop.GetValue(item)
                };

                SetConditionModels.Add(conditionModel);
            }


            TableName = PostgresMapping.GetTableName<I>();
            StoredProcedureName = PostgresMapping.GetProcedureName<I>(QueryType.Update, TableName, WhereConditionModels, SetConditionModels);

            foreach (ConditionModel condition in WhereConditionModels ?? Enumerable.Empty<ConditionModel>())
            {
                PostgresMapping.AddParameter(_dynamicParameters, condition, "_w");
            }

            foreach (ConditionModel condition in SetConditionModels)
            {
                PostgresMapping.AddParameter(_dynamicParameters, condition, "_s");
            }
        }


        public async Task<List<I>> UpdateAsync()
        {
            using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            Stopwatch stopwatch = StopWatchStart();

            IEnumerable<C> obj;

            try
            {
                if (_item != null && _item is IDBObject itemOnUpdate)
                {
                    await itemOnUpdate.OnUpdate(_dBAutomator);
                }

                obj = await connection.QueryAsync<C>(StoredProcedureName, _dynamicParameters, _dbTransaction, _commandTimeout, CommandType.StoredProcedure);

                var result = obj.Cast<I>().ToList();

                if (_item != null && _item is IDBObject itemOnUpdated)
                {
                    itemOnUpdated.IsDirty = false;

                    itemOnUpdated.IsNewRecord = false;

                    await itemOnUpdated.OnUpdated(_dBAutomator);
                }
                else
                {
                    foreach (I item in result)
                    {
                        if (item is IDBObject dBObject)
                        {
                            dBObject.IsDirty = false;

                            dBObject.IsNewRecord = false;

                            await dBObject.OnUpdated(_dBAutomator);
                        }
                    }
                }

                return result;
            }
            catch (PostgresException e)
            {
                if (e.SqlState == "42883")  //function does not exist
                {
                    CreateUpdate(connection);

                    obj = await connection.QueryAsync<C>(StoredProcedureName, _dynamicParameters, _dbTransaction, _commandTimeout, CommandType.StoredProcedure);

                    var result = obj.Cast<I>().ToList();

                    if (_item != null && _item is IDBObject itemOnUpdated)
                    {
                        await itemOnUpdated.OnUpdated(_dBAutomator);
                    }
                    else
                    {
                        foreach (I item in result)
                        {
                            if (item is IDBObject dBObject)
                            {
                                dBObject.IsDirty = false;

                                dBObject.IsNewRecord = false;

                                await dBObject.OnUpdated(_dBAutomator);
                            }
                        }
                    }

                    return result;
                }

                _dBAutomator.Logger?.LogWarning(LoggingEvents.ErrorExecutingQuery, "{source}: {method} {message}", _source, "UpdateAsync", e.Message);
                
                throw;
            }

            finally
            {
                StopWatchEnd(stopwatch, "InsertAsync()");

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











        private void CreateUpdate(NpgsqlConnection connection)
        {
            _dBAutomator.Logger?.LogDebug(LoggingEvents.ModifyingDatabase, "{source}: {method}", _source, "CreateUpdate");

            using NpgsqlCommand command = new NpgsqlCommand
            {
                Connection = connection
            };

            string commandText = "";

            commandText = $"{commandText}CREATE OR REPLACE FUNCTION {StoredProcedureName}(";


            foreach (ConditionModel condition in WhereConditionModels ?? Enumerable.Empty<ConditionModel>())
            {
                if (condition.Value == null)
                {
                    throw new ArgumentNullException();
                }
                
                commandText = $"{commandText}_w{condition.Name.ToLower()} {condition.Value.MapToPostgreSql()}\n, ";
            }

            foreach (ConditionModel condition in SetConditionModels)
            {
                if (condition.Value == null)
                {
                    throw new ArgumentNullException();
                }

                commandText = $"{commandText}_s{condition.Name.ToLower()} {condition.Value.MapToPostgreSql()}\n, ";
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
                    commandText = $"{commandText}{PostgresMapping.GetColumnName<I>(property.Name)} {property.MapToPostgreSql()}\n, ";
                }
            }

            commandText = commandText.Substring(0, commandText.Length - 2);

            commandText = $"{commandText})\n";

            commandText = $"{commandText}LANGUAGE SQL\n";

            commandText = $"{commandText}AS $$\n\n";

            commandText = $"{commandText}UPDATE \"{TableName}\"\n";

            commandText = $"{commandText}SET \n";

            foreach (ConditionModel condition in SetConditionModels)
            {
                commandText = $"{commandText}\"{PostgresMapping.GetColumnName<I>(condition.Name)}\" = _s{condition.Name.ToLower()}\n, ";
            }

            commandText = commandText.Left(commandText.Length - 2);

            commandText = $"{commandText}\n";

            if (WhereConditionModels != null)
            {
                commandText = $"{commandText}WHERE\n";

                foreach (ConditionModel condition in WhereConditionModels ?? Enumerable.Empty<ConditionModel>())
                {
                    commandText = $"{commandText}\"{TableName}\".\"{PostgresMapping.GetColumnName<I>(condition.Name)}\" {condition.Operator} _w{condition.Name.ToLower()} AND ";
                }
                commandText = commandText.Substring(0, commandText.Length - 5);

            }

            commandText = $"{commandText}\n";

            commandText = $"{commandText}RETURNING *\n";

            commandText = $"{commandText}\n$$\n";

            command.CommandText = commandText;

            command.ExecuteNonQuery();

        }


    }
}
