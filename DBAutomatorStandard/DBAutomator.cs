using Dapper;
using DBAutomatorStandard;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static DBAutomatorStandard.Enums;

namespace DBAutomatorLibrary
{
    public delegate void IsAvailableChangedEventHandler(bool isAvailable);

    public delegate void SlowQueryWarningEventHandler(string methodName, TimeSpan timeSpan);




    public class DBAutomator
    {
        public event SlowQueryWarningEventHandler? OnSlowQueryDetected;

        public readonly ILogger? Logger;



        private readonly string _connectionString;

        private readonly int _slowQueryWarningInSeconds;

        private readonly DataStore _dataStore;

        private const string _source = nameof(DBAutomator);

        public readonly List<RegisteredClass> RegisteredClasses = new List<RegisteredClass>();

        public DBAutomator(DataStore dataStore, string connectionString, int slowQueryWarningInSeconds = 5, ILogger? logger = null)
        {
            Logger = logger;

            _dataStore = dataStore;
            _connectionString = connectionString;
            _slowQueryWarningInSeconds = slowQueryWarningInSeconds;
        }

        public void Register(object someObject)
        {
            var registeredClass = new RegisteredClass(someObject);

            RegisteredClasses.Add(registeredClass);
        }


        internal void SlowQueryDetected(string methodName, TimeSpan timeSpan)
        {
            OnSlowQueryDetected?.Invoke(methodName, timeSpan);

            Logger?.LogWarning(LoggingEvents.SlowQuery, "{source}: Slow Query {methodName} took {seconds} seconds.", _source, methodName, (int)timeSpan.TotalSeconds);
        }





        public async Task<I> GetAsync<I, C>(Expression<Func<C, object>> where, IDbTransaction? dbTransaction = null, int? commandTimeout = null) where C : I
        {
            Logger?.LogDebug(LoggingEvents.QueryingDatabase, "{source}: {method} {type}", _source, "GetAsync", typeof(C));

            ISelectQuery<I, C> query;

            if (_dataStore == DataStore.PostgreSQL)
            {
                query = new PostgresSelectQuery<I, C>(this, _connectionString, _slowQueryWarningInSeconds, where, dbTransaction, commandTimeout);
            }
            else
            {
                throw new NotImplementedException();
            }

            return await query.GetAsync();

        }

        public async Task<List<I>> GetListAsync<I, C>(Expression<Func<C, object>>? where = null, IDbTransaction? dbTransaction = null, int? commandTimeout = null) where C : I
        {
            Logger?.LogDebug(LoggingEvents.QueryingDatabase, "{source}: {method} {type}", _source, "GetListAsync", typeof(C));

            ISelectQuery<I, C> query;

            if (_dataStore == DataStore.PostgreSQL)
            {
                query = new PostgresSelectQuery<I, C>(this, _connectionString, _slowQueryWarningInSeconds, where, dbTransaction, commandTimeout);
            }
            else
            {
                throw new NotImplementedException();
            }

            return await query.GetListAsync();

        }

        public async Task InsertAsync<I, C>(I item, IDbTransaction? dbTransaction = null, int? commandTimeout = null) where C : I where I : class
        {
            Logger?.LogDebug(LoggingEvents.QueryingDatabase, "{source}: {method} {type}", _source, "InsertAsync", typeof(C));

            IInsertQuery<I, C> query;

            if (_dataStore == DataStore.PostgreSQL)
            {
                //query = new PostgresInsertQuery<I, C>(item, this, _connectionString, _slowQueryWarningInSeconds, dbTransaction, commandTimeout);
                var abc = new PostgresInsertQuery(item, this, _connectionString, _slowQueryWarningInSeconds, dbTransaction, commandTimeout);
            }
            else
            {
                throw new NotImplementedException();
            }

            //await query.InsertAsync();
        }

        public async Task<List<I>> DeleteAsync<I, C>(Expression<Func<C, object>>? where = null, IDbTransaction? dbTransaction = null, int? commandTimeout = null) where C : I where I : class
        {
            Logger?.LogDebug(LoggingEvents.QueryingDatabase, "{source}: {method} {type}", _source, "DeleteAsync", typeof(C));

            IDeleteQuery<I, C> query;

            if (_dataStore == DataStore.PostgreSQL)
            {
                query = new PostgresDeleteQuery<I, C>(this, _connectionString, _slowQueryWarningInSeconds, where, dbTransaction, commandTimeout);
            }
            else
            {
                throw new NotImplementedException();
            }

            return await query.DeleteAsync();
        }

        public async Task<List<I>> DeleteAsync<I, C>(I item, IDbTransaction? dbTransaction = null, int? commandTimeout = null) where C : I where I : class
        {
            Logger?.LogDebug(LoggingEvents.QueryingDatabase, "{source}: {method} {type}", _source, "DeleteAsync", typeof(C));

            IDeleteQuery<I, C> query;

            if (_dataStore == DataStore.PostgreSQL)
            {
                query = new PostgresDeleteQuery<I, C>(item, this, _connectionString, _slowQueryWarningInSeconds, dbTransaction, commandTimeout);
            }
            else
            {
                throw new NotImplementedException();
            }

            return await query.DeleteAsync();
        }

        public async Task<List<I>> UpdateAsync<I, C>(Expression<Func<C, object>> set, Expression<Func<C, object>>? where = null, IDbTransaction? dbTransaction = null, int? commandTimeout = null) where C : I where I : class
        {
            Logger?.LogDebug(LoggingEvents.QueryingDatabase, "{source}: {method} {type}", _source, "UpdateAsync", typeof(C));

            IUpdateQuery<I, C> query;

            if (_dataStore == DataStore.PostgreSQL)
            {
                query = new PostgresUpdateQuery<I, C>(this, _connectionString, _slowQueryWarningInSeconds, set, where, dbTransaction, commandTimeout);
            }
            else
            {
                throw new NotImplementedException();
            }

            return await query.UpdateAsync();
        }

        public async Task<List<I>> UpdateAsync<I, C>(I item, IDbTransaction? dbTransaction = null, int? commandTimeout = null) where C : I where I : class
        {
            Logger?.LogDebug(LoggingEvents.QueryingDatabase, "{source}: {method} {type}", _source, "UpdateAsync", typeof(C));

            IUpdateQuery<I, C> query;

            if (_dataStore == DataStore.PostgreSQL)
            {
                query = new PostgresUpdateQuery<I, C>(item, this, _connectionString, _slowQueryWarningInSeconds, dbTransaction, commandTimeout);
            }
            else
            {
                throw new NotImplementedException();
            }

            return await query.UpdateAsync();
        }






        public async Task DeleteAllWithZPrefix()
        {
            Logger?.LogWarning(LoggingEvents.ModifyingDatabase, "{source}: {method}", _source, "DeleteAllWithZPrefix");

            using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            NpgsqlCommand command = new NpgsqlCommand
            {
                Connection = connection,

                CommandText = $@"SELECT 'DROP FUNCTION ' || ns.nspname || '.' || proname || '(' || oidvectortypes(proargtypes) || ');'
                                        FROM pg_proc INNER JOIN pg_namespace ns ON(pg_proc.pronamespace = ns.oid)
                                        WHERE ns.nspname = 'public' AND proname Like 'z%'
                                        order by proname;"
            };

            System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                using NpgsqlConnection deleteConnection = new NpgsqlConnection(_connectionString);

                NpgsqlCommand deleteCommand = new NpgsqlCommand();

                await deleteConnection.OpenAsync();

                deleteCommand.Connection = deleteConnection;

                deleteCommand.CommandText = reader.GetString(0);

                await deleteCommand.ExecuteNonQueryAsync();

                deleteConnection.Close();
            }

            connection.Close();
        }
    }
}
