using Dapper.SqlWriter.Interfaces;
using Dapper.SqlWriter.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.SqlWriter
{
    public class SqlWriter : IDisposable
    {
        public delegate Task SlowQueryEventHandler(object sender, SlowQueryEventArgs e);

        public delegate Task QueryFailureEventHandler(object sender, QueryFailureEventArgs e);

        public event SlowQueryEventHandler? SlowQuery;

        public event QueryFailureEventHandler? QueryFailure;

        internal SemaphoreSlim SemaphoreSlim { get; } = new SemaphoreSlim(1);

        public List<object> RegisteredClasses { get; } = new List<object>();

        internal ISqlWriterConfiguration Config { get; }

        public SqlWriter(ISqlWriterConfiguration config) => Config = config;
        
        public RegisteredClass<C> Register<C>() where C : class
        {
            try
            {
                var registeredClass = new RegisteredClass<C>(this);

                RegisteredClasses.Add(registeredClass);

                return registeredClass;
            }
            catch (Exception e)
            {
                throw new SqlWriterException(e.Message, e);
            }
        }

        public RegisteredClass<C> Register<C>(string tableName) where C : class
        {
            var item = Register<C>();

            item.DatabaseTableName = tableName;

            return item;
        }

        internal void OnSlowQuery(object query, TimeSpan timeSpan, string sqlInjectString) => SlowQuery?.Invoke(this, new SlowQueryEventArgs(query, timeSpan, sqlInjectString));

        internal void OnQueryFailure(object query, QueryFailure queryFailure) => QueryFailure?.Invoke(this, new QueryFailureEventArgs(query, queryFailure));

        public Get<C> Select<C>() where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.FirstOrDefault(r => r is RegisteredClass<C>) ?? Register<C>();

            return new Get<C>(registeredClass, this);
        }

        public Delete<C> Delete<C>() where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.FirstOrDefault(r => r is RegisteredClass<C>) ?? Register<C>();
            
            return new Delete<C>(registeredClass, this);
        }

        public Delete<C> Delete<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.First(r => r is RegisteredClass<C>) ?? Register<C>();

            return new Delete<C>(item, registeredClass, this);
        }

        public Insert<C> Insert<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.FirstOrDefault(r => r is RegisteredClass<C>) ?? Register<C>();

            return new Insert<C>(item, registeredClass, this);
        }

        public Update<C> Update<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.FirstOrDefault(r => r is RegisteredClass<C>) ?? Register<C>();

            return new Update<C>(item, registeredClass, this);
        }

        public Update<C> Update<C>() where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.FirstOrDefault(r => r is RegisteredClass<C>) ?? Register<C>();

            return new Update<C>(registeredClass, this);
        }

        public void Dispose()
        {
            SemaphoreSlim.Dispose();
        }

        /// <summary>
        /// The function returns the expected table name when you don't explicity provide it.
        /// It defaults to the name of your class. You may set this function equal to your own to handle your
        /// class name to table name conversion.
        /// </summary>
        public Func<string, string> ToTableName { get; set; } = DefaultTableName;

        private static string DefaultTableName(string arg) => arg;

        public Capitalization Capitalization { get; set; } = Capitalization.Default;

        internal List<PropertyMap> PropertyMaps { get; } = new List<PropertyMap>();

        /// <summary>
        /// Configure default type conversions to convert a type to a database column.
        /// </summary>
        /// <param name="p"></param>
        public void AddPropertyMap(PropertyMap p) => PropertyMaps.Add(p);   
    }

    public interface ISqlWriterConfiguration
    {
        IDbConnection CreateDbConnection();

        TimeSpan SlowQueryWarning { get; }

        int? CommandTimeOut { get; }

        bool AllowConcurrentQueries { get; }
    }
}
