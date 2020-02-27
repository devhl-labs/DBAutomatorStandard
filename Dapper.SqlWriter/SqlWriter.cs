﻿using Dapper.SqlWriter.Interfaces;
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
            var a = typeof(C);

            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            return new Insert<C>(item, registeredClass, this, Connection, QueryOptions, Logger);
        }

        //public Insert<T> Insert<T>(T item) where T : DBObject<T>
        //{
        //    var a = typeof(T);

        //    RegisteredClass<T> registeredClass = (RegisteredClass<T>) RegisteredClasses.First(r => r is RegisteredClass<C>);

        //    return new Insert<T>(item, registeredClass, this, Connection, QueryOptions, Logger);
        //}

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

        internal string GetValues<C>(C obj)
        {
            RegisteredClass<C> registeredClass = (RegisteredClass<C>) RegisteredClasses.First(r => r is RegisteredClass<C>);

            string result = string.Empty;

            foreach(var prop in registeredClass.RegisteredProperties.Where(p => !p.NotMapped).OrderBy(p => p.PropertyName))
            {
                result = $"{result}{prop.Property.GetValue(obj, null)};";
            }

            return result[..^1];
        }

        public ObjectState GetObjectState<C>(C item) where C : DBObject<C>
        {
            if (item.ObjectState == ObjectState.Deleted) return ObjectState.Deleted;

            if (item.ObjectState == ObjectState.New) return ObjectState.New;

            if (item.ObjectState == ObjectState.Dirty) return ObjectState.Dirty;

            if (item._oldValues != GetValues(item)) item.ObjectState = ObjectState.Dirty;

            return item.ObjectState;
        }

        public void Dispose()
        {
            _semaphoreSlim.Dispose();
        }
    }
}
