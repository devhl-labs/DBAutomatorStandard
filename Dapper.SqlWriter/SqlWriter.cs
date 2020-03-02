using Dapper.SqlWriter.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Dapper.SqlWriter
{
    public delegate void IsAvailableChangedEventHandler(bool isAvailable);


    public delegate void SlowQueryWarningEventHandler(string methodName, TimeSpan timeSpan);


    public class SqlWriter : IDisposable
    {
        internal readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        public event SlowQueryWarningEventHandler? OnSlowQueryDetected;

        public ILogger? Logger { get; }


        private const string _source = nameof(SqlWriter);


        public readonly List<object> RegisteredClasses = new List<object>();

        public QueryOptions QueryOptions { get; } = new QueryOptions();

        public IDbConnection Connection { get; }

        public SqlWriter(IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            Logger = logger;

            QueryOptions = queryOptions;

            Connection = connection;
        }

        public RegisteredClass<C> Register<C>() where C : class
        {
            try
            {
                var registeredClass = new RegisteredClass<C>();

                RegisteredClasses.Add(registeredClass);

                return registeredClass;
            }
            catch (Exception e)
            {
                throw new SqlWriterException(e.Message, e);
            }
        }

        //public RegisteredClass<T> Register<T>() where T : DBObject<T>
        //{
        //    try
        //    {
        //        var registeredClass = new RegisteredClass<T>();

        //        RegisteredClasses.Add(registeredClass);

        //        return registeredClass;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new SqlWriterException(e.Message, e);
        //    }
        //}

        public RegisteredClass<C> Register<C>(string tableName) where C : class
        {
            var item = Register<C>();

            item.DatabaseTableName = tableName;

            return item;            
        }

        internal void SlowQueryDetected(string sql, TimeSpan timeSpan) => OnSlowQueryDetected?.Invoke(sql, timeSpan);

        public Select<C> Select<C>() where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Select<C>(registeredClass, this, Connection, QueryOptions, Logger);
        }

        public DeleteBase<C> Delete<C>() where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new DeleteBase<C>(registeredClass, this, Connection, QueryOptions, Logger);
        }

        public DeleteBase<C> Delete<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new DeleteBase<C>(item, registeredClass, this, Connection, QueryOptions, Logger);
        }

        public InsertBase<C> Insert<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new InsertBase<C>(item, registeredClass, this, Connection, QueryOptions, Logger);
        }

        public UpdateBase<C> Update<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new UpdateBase<C>(item, registeredClass, this, Connection, QueryOptions, Logger);
        }

        public UpdateBase<C> Update<C>() where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new UpdateBase<C>(registeredClass, this, Connection, QueryOptions, Logger);
        }

        public void Dispose()
        {
            _semaphoreSlim.Dispose();
        }
    }
}
