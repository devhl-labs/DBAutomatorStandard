using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Dapper.SqlWriter.Interfaces;
using System.Data;

namespace Dapper.SqlWriter
{
    public sealed class Insert<C> : BaseQuery<C> where C : class
    {
        private C Item { get; }

        internal Insert(C item, RegisteredClass<C> registeredClass, SqlWriter sqlWriter)
        {
            SqlWriter = sqlWriter;

            RegisteredClass = registeredClass;

            Item = item ?? throw new SqlWriterException("Item must not be null.", new ArgumentException());

            Utils.AddParameters(P, Item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped));
        }

        public Insert<C> TimeOut(int value)
        {
            CommandTimeOut = value;

            return this;
        }

        public string ToSqlInjectionString() => GetString(true);

        public override string ToString() => GetString();

        private string GetString(bool allowSqlInjection = false)
        {
            string sql = $"INSERT INTO \"{RegisteredClass.DatabaseTableName}\" (";

            foreach (var property in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) 
                sql = $"{sql}\"{property.ColumnName}\", ";

            sql = sql[0..^2];

            sql = $"{sql}) VALUES (";

            if (allowSqlInjection)
            {
                foreach (RegisteredProperty<C> registeredProperty in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement))
                {
                    if (registeredProperty.Property.PropertyType.GetType() == typeof(string))                    
                        sql = $"{sql}'{registeredProperty.Property.GetValue(Item, null)}', ";                    
                    else                    
                        sql = $"{sql}{registeredProperty.Property.GetValue(Item, null)}, ";                    
                }
            }
            else
            {
                foreach (var property in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) 
                    sql = $"{sql}@w_{property.ColumnName}, ";
            }

            sql = sql[0..^2];

            sql = $"{sql}) ";

            return sql;
        }

        public async Task<C> QueryFirstAsync()
        {
            if (Item is IDBEvent dBEvent) 
                _ = dBEvent.OnInsertAsync(SqlWriter);

            var result = await QueryFirstAsync(QueryType.Insert, $"{ToString()}RETURNING *;").ConfigureAwait(false);

            Utils.FillDatabaseGeneratedProperties(Item, result, SqlWriter);

            if (Item is DBObject dbObject)
            {
                dbObject.ObjectState = ObjectState.InDatabase;

                dbObject.QueryType = QueryType.Insert;

                dbObject.StoreState<C>(SqlWriter);
            }

            if (Item is IDBEvent dBEvent1) 
                _ = dBEvent1.OnInsertedAsync(SqlWriter);



            return result;
        }

        public async Task ExecuteAsync()
        {
            if (Item is IDBEvent dBEvent)
                _ = dBEvent.OnInsertAsync(SqlWriter);

            await ExecuteAsync($"{ToString()};").ConfigureAwait(false);

            if (Item is DBObject dbObject)
            {
                dbObject.ObjectState = ObjectState.InDatabase;

                dbObject.QueryType = QueryType.Insert;

                dbObject.StoreState<C>(SqlWriter);
            }

            if (Item is IDBEvent dBEvent1)
                _ = dBEvent1.OnInsertedAsync(SqlWriter);
        }
    }
}
