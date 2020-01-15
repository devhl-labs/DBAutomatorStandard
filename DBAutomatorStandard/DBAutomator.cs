using devhl.DBAutomator.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace devhl.DBAutomator
{
    public delegate void IsAvailableChangedEventHandler(bool isAvailable);


    public delegate void SlowQueryWarningEventHandler(string methodName, TimeSpan timeSpan);


    public class DBAutomator
    {
        public event SlowQueryWarningEventHandler? OnSlowQueryDetected;

        public ILogger? Logger { get; }


        private const string _source = nameof(DBAutomator);


        public readonly List<object> RegisteredClasses = new List<object>();

        public QueryOptions QueryOptions { get; } = new QueryOptions();
        public IDbConnection Connection { get; }

        public DBAutomator(IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
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
                throw new DbAutomatorException(e.Message, e);
            }


        }

        public RegisteredClass<C> Register<C>(string tableName) where C : class
        {
            var item = Register<C>();

            item.TableName = tableName;

            return item;            
        }

        internal void SlowQueryDetected(string sql, TimeSpan timeSpan) => OnSlowQueryDetected?.Invoke(sql, timeSpan);

        public Select<C> Select<C>(Expression<Func<C, object>>? select = null) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Select<C>(select, registeredClass, this, Connection, QueryOptions, Logger);
        }

        public Delete<C> Delete<C>() where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Delete<C>(registeredClass, this, Connection, QueryOptions, Logger);
        }

        public Delete<C> Delete<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Delete<C>(item, registeredClass, this, Connection, QueryOptions, Logger);
        }

        public Insert<C> Insert<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Insert<C>(item, registeredClass, this, Connection, QueryOptions, Logger);
        }

        public Update<C> Update<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Update<C>(item, registeredClass, this, Connection, QueryOptions, Logger);
        }

        public Update<C> Update<C>() where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Update<C>(registeredClass, this, Connection, QueryOptions, Logger);
        }
    }
}
