using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Dapper.SqlWriter.Interfaces;
using System.Data;

namespace Dapper.SqlWriter
{
    public class InsertBase<C> : BaseQuery<C> where C : class
    {
        protected C _item;

        internal InsertBase(C item, RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _sqlWriter = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _connection = connection;

            _item = item ?? throw new SqlWriterException("Item must not be null.", new ArgumentException());

            Statics.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped));
        }

        public InsertBase<C> Options(QueryOptions queryOptions)
        {
            _queryOptions = queryOptions;

            return this;
        }

        public string ToSqlInjectionString() => GetString(true);

        public override string ToString() => GetString();

        private string GetString(bool allowSqlInjection = false)
        {
            string sql = $"INSERT INTO \"{_registeredClass.DatabaseTableName}\" (";

            foreach (var property in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) sql = $"{sql}\"{property.ColumnName}\", ";

            sql = sql[0..^2];

            sql = $"{sql}) VALUES (";

            if (allowSqlInjection)
            {
                foreach (RegisteredProperty<C> registeredProperty in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement))
                {
                    if (registeredProperty.PropertyType.GetType() == typeof(string))
                    {
                        sql = $"{sql}'{registeredProperty.Property.GetValue(_item, null)}', ";
                    }
                    else
                    {
                        sql = $"{sql}{registeredProperty.Property.GetValue(_item, null)}, ";
                    }
                }
            }
            else
            {
                foreach (var property in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) sql = $"{sql}@w_{property.ColumnName}, ";
            }

            sql = sql[0..^2];

            sql = $"{sql}) RETURNING *;";

            return sql;
        }

        public async Task<C> QueryFirstAsync()
        {
            if (_item is IDBEvent dBEvent) _ = dBEvent.OnInsertAsync(_sqlWriter);

            var result = await QueryFirstAsync(QueryType.Insert, ToString()).ConfigureAwait(false);

            if (_item is IDBEvent dBEvent1) _ = dBEvent1.OnInsertedAsync(_sqlWriter);

            return result;
        }
    }
}
