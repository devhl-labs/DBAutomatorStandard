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
//using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static DBAutomatorLibrary.Statics;
using static DBAutomatorStandard.Enums;

namespace DBAutomatorLibrary
{
    internal class PostgresInsertQuery //<I, C> : IInsertQuery<I, C> where C : I where I : class
    {
        //private const string _source = nameof(PostgresInsertQuery<I, C>);
        private readonly string _connectionString;
        private readonly IDbTransaction? _dbTransaction;
        private readonly int? _commandTimeout;
        private readonly DBAutomator _dBAutomator;
        private readonly int _slowQueryWarningInSeconds;
        private readonly DynamicParameters _dynamicParameters = new DynamicParameters();
        //private readonly string _schema;
        //private readonly bool _hasGeneratedColumn = false;
        //private readonly PropertyInfo? _generatedColumn;
        //private readonly I? _item;

        public string TableName { get; }
        public string StoredProcedureName { get; }

        public PostgresInsertQuery(object item, DBAutomator dBAutomator, string connectionString, int slowQueryWarningInSeconds, IDbTransaction? dbTransaction = null, int? commandTimeout = null)
        {
            DynamicParameters p = new DynamicParameters();

            RegisteredClass registeredClass = dBAutomator.RegisteredClasses.First(r => r.SomeClass.GetType() == item.GetType());

            string sql = $"INSERT INTO {registeredClass.TableName} (";

            foreach(var property in registeredClass.RegisteredProperties)
            {
                sql = $"{sql}\"{property.ColumnName}\", ";

                if (property.PropertyType == typeof(ulong))
                {
                    p.Add(property.ColumnName, Convert.ToInt64(item.GetType().GetProperty(property.PropertyName).GetValue(item)));
                }
                else
                {
                    p.Add(property.ColumnName, item.GetType().GetProperty(property.PropertyName).GetValue(item));
                }

            }

            sql = sql[0..^2];

            sql = $"{sql}) VALUES (";

            foreach(var property in registeredClass.RegisteredProperties)
            {
                sql = $"{sql}@{property.ColumnName}, ";
            }

            sql = sql[0..^2];

            sql = $"{sql});";

            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);

            connection.Open();

            connection.Execute(sql, p);

            connection.Close();

        }

        //public PostgresInsertQuery(I item, DBAutomator dBAutomator, string connectionString, int slowQueryWarningInSeconds, IDbTransaction? dbTransaction = null, int? commandTimeout = null)
        //{
            //_dBAutomator = dBAutomator;
            //_connectionString = connectionString;
            //_slowQueryWarningInSeconds = slowQueryWarningInSeconds;
            //_dbTransaction = dbTransaction;
            //_commandTimeout = commandTimeout;
            ////_schema = schema;
            //_item = item;

            //TableName = PostgresMapping.GetTableName<I>().ToLower();
            //StoredProcedureName = $"z{TableName}_ins";

            //foreach (PropertyInfo property in typeof(I).GetProperties())
            //{
            //    if (property.GetCustomAttributes<IdentityAttribute>(true).FirstOrDefault() is IdentityAttribute databaseGenerated)
            //    {
            //        _generatedColumn = property;
            //        continue;
            //    }

            //    PostgresMapping.AddParameter(_dynamicParameters, property, item);
            //}
        //}

        //public async Task InsertAsync()
        //{
        //    if (_item is IDBObject dBObject)
        //    {
        //        await dBObject.OnInsert(_dBAutomator);
        //    }

        //    if (_generatedColumn == null)
        //    {
        //        await InsertBasicAsync();                
        //    }
        //    else
        //    {
        //        await InsertAndGetID();
        //    }

        //    if (_item is IDBObject insertedDbObject)
        //    {
        //        insertedDbObject.IsDirty = false;

        //        insertedDbObject.IsNewRecord = false;

        //        await insertedDbObject.OnInserted(_dBAutomator);
        //    }
        //}

        //private async Task InsertAndGetID()
        //{
        //    if(_generatedColumn == null)
        //    {
        //        throw new Exception("Generated column cannot be null.");
        //    }

        //    using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);

        //    await connection.OpenAsync();

        //    Stopwatch stopwatch = StopWatchStart();

        //    try
        //    {
        //        _generatedColumn.SetValue(_item, await connection.ExecuteScalarAsync(StoredProcedureName, _dynamicParameters, _dbTransaction, _commandTimeout, CommandType.StoredProcedure));

        //        return;
        //    }
        //    catch (PostgresException e)
        //    {
        //        if (e.SqlState == "42883")  //function does not exist
        //        {
        //            CreateInsertAndGetID(connection);

        //            _generatedColumn?.SetValue(_item, await connection.ExecuteScalarAsync(StoredProcedureName, _dynamicParameters, _dbTransaction, _commandTimeout, CommandType.StoredProcedure));

        //            return;
        //        }

        //        _dBAutomator.Logger?.LogWarning(LoggingEvents.ErrorExecutingQuery, "{source}: {method} {message}", _source, "InsertAndGetID", e.Message);

        //        throw;
        //    }

        //    finally
        //    {
        //        StopWatchEnd(stopwatch, "InsertAsync()");

        //        connection.Close();
        //    }
        //}

        //private async Task InsertBasicAsync()
        //{
        //    using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);

        //    await connection.OpenAsync();

        //    Stopwatch stopwatch = StopWatchStart();

        //    try
        //    {
        //        await connection.ExecuteAsync(StoredProcedureName, _dynamicParameters, _dbTransaction, _commandTimeout, CommandType.StoredProcedure);

        //        return;
        //    }
        //    catch (PostgresException e)
        //    {
        //        if (e.SqlState == "42883")  //function does not exist
        //        {
        //            CreateInsert(connection);

        //            await connection.ExecuteAsync(StoredProcedureName, _dynamicParameters, _dbTransaction, _commandTimeout, CommandType.StoredProcedure);

        //            return;
        //        }
                
        //        _dBAutomator.Logger?.LogWarning(LoggingEvents.ErrorExecutingQuery, "{source}: {method} {message}", _source, "InsertBasicAsync", e.Message);

        //        throw;
        //    }

        //    finally
        //    {
        //        StopWatchEnd(stopwatch, "InsertAsync()");

        //        connection.Close();
        //    }
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
            if (stopwatch.Elapsed.TotalSeconds > _slowQueryWarningInSeconds)
            {
                _dBAutomator.SlowQueryDetected(methodName, stopwatch.Elapsed);
            }
        }








        //private void CreateInsert(NpgsqlConnection connection)
        //{
        //    _dBAutomator.Logger?.LogDebug(LoggingEvents.ModifyingDatabase, "{source}: {method} {type}", _source, "CreateInsert", typeof(C));

        //    using NpgsqlCommand command = new NpgsqlCommand
        //    {
        //        Connection = connection
        //    };

        //    string commandText = "";

        //    commandText = $"{commandText}CREATE OR REPLACE FUNCTION z{TableName}_Ins(";

        //    foreach (PropertyInfo property in typeof(I).GetProperties())
        //    {
        //        if (property.IsStorable())
        //        {
        //            commandText = $"{commandText}_{property.Name} {property.MapToPostgreSql()}\n, ";
        //        }
        //    }

        //    commandText = commandText.Substring(0, commandText.Length - 2);
        //    commandText = $"{commandText})\n";


        //    commandText = $"{commandText}RETURNS void\n";

        //    commandText = $"{commandText}LANGUAGE SQL\n";

        //    commandText = $"{commandText}AS $$\n\n";

        //    commandText = $"{commandText}INSERT INTO \"{TableName}\" (\n";

        //    foreach (PropertyInfo property in typeof(I).GetProperties())
        //    {
        //        if (property.IsStorable())
        //        {
        //            commandText = $"{commandText}\"{PostgresMapping.GetColumnName<I>(property.Name)}\"\n, ";
        //        }
        //    }

        //    commandText = commandText.Substring(0, commandText.Length - 2);

        //    commandText = $"{commandText})\n";

        //    commandText = $"{commandText}VALUES(\n";

        //    foreach (PropertyInfo property in typeof(I).GetProperties())
        //    {
        //        if (property.IsStorable())
        //        {
        //            commandText = $"{commandText}_{property.Name.ToLower()}\n, ";
        //        }
        //    }
        //    commandText = commandText.Substring(0, commandText.Length - 2);

        //    commandText = $"{commandText});\n\n";

        //    commandText = $"{commandText}$$\n";

        //    command.CommandText = commandText;
        //    command.ExecuteNonQuery();

        //}

        //private void CreateInsertAndGetID(NpgsqlConnection connection)
        //{
        //    _dBAutomator.Logger?.LogDebug(LoggingEvents.ModifyingDatabase, "{source}: {method} {type}", _source, "CreateInsertAndGetId", typeof(C));

        //    if (_generatedColumn == null)
        //    {
        //        throw new Exception("The generated column is not found.");
        //    }

        //    using NpgsqlCommand command = new NpgsqlCommand
        //    {
        //        Connection = connection
        //    };

        //    string commandText = "";

        //    commandText = $"{commandText}CREATE OR REPLACE FUNCTION z{TableName}_Ins(";

        //    foreach (PropertyInfo property in typeof(I).GetProperties())
        //    {
        //        if (property != _generatedColumn && property != _generatedColumn && property.IsStorable())
        //        {
        //            commandText = $"{commandText}_{property.Name} {property.MapToPostgreSql()}\n, ";
        //        }
        //    }

        //    commandText = commandText.Substring(0, commandText.Length - 2);
        //    commandText = $"{commandText})\n";


        //    commandText = $"{commandText}RETURNS {PostgresMapping.MapToPostgreSql(_generatedColumn)}\n";

        //    commandText = $"{commandText}LANGUAGE SQL\n";

        //    commandText = $"{commandText}AS $$\n\n";

        //    commandText = $"{commandText}INSERT INTO \"{TableName}\" (\n";

        //    foreach (PropertyInfo property in typeof(I).GetProperties())
        //    {
        //        if (property.IsStorable())
        //        {
        //            commandText = $"{commandText}\"{PostgresMapping.GetColumnName<I>(property.Name)}\"\n, ";
        //        }
        //    }

        //    commandText = commandText.Substring(0, commandText.Length - 2);

        //    commandText = $"{commandText})\n";
        //    commandText = $"{commandText}VALUES(\n";

        //    foreach (PropertyInfo property in typeof(I).GetProperties())
        //    {
        //        if (property == _generatedColumn)
        //        {

        //            commandText = $"{commandText}default\n, ";
        //        }
        //        else if (property.IsStorable())
        //        {
        //            commandText = $"{commandText}_{property.Name.ToLower()}\n, ";
        //        }
        //    }
        //    commandText = commandText.Substring(0, commandText.Length - 2);

        //    commandText = $"{commandText})\n";
        //    commandText = $"{commandText}RETURNING \"{PostgresMapping.GetColumnName<I>(_generatedColumn.Name)}\";\n\n";

        //    commandText = $"{commandText}$$";

        //    command.CommandText = commandText;
        //    command.ExecuteNonQuery();
        //}
    }
}
