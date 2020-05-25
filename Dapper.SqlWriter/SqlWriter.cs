﻿using Dapper.SqlWriter.Interfaces;
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

        internal SemaphoreSlim SemaphoreSlim { get; } = new SemaphoreSlim(1);

        public event SlowQueryEventHandler SlowQuery;

        public ILogger? Logger { get; }


        public List<object> RegisteredClasses { get; } = new List<object>();

        public SqlWriterConfiguration QueryOptions { get; } = new SqlWriterConfiguration();

        public IDbConnection Connection { get; }

        public SqlWriter(IDbConnection connection, SqlWriterConfiguration queryOptions, ILogger? logger = null)
        {
            Logger = logger;

            QueryOptions = queryOptions;

            Connection = connection;
        }

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


        public Select<C> Select<C>() where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Select<C>(registeredClass, this, Connection, QueryOptions, Logger);
        }

        public Delete<C> Delete<C>() where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Delete<C>(registeredClass, this, Connection, QueryOptions, Logger);
        }

        public Delete<C> Delete<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Delete<C>(item, registeredClass, this, Connection, QueryOptions, Logger);
        }

        public Insert<C> Insert<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Insert<C>(item, registeredClass, this, Connection, QueryOptions, Logger);
        }

        public Update<C> Update<C>(C item) where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Update<C>(item, registeredClass, this, Connection, QueryOptions, Logger);
        }

        public Update<C> Update<C>() where C : class
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>)RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Update<C>(registeredClass, this, Connection, QueryOptions, Logger);
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
}
