using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Dapper.SqlWriter.Interfaces;
using System.Data;

namespace Dapper.SqlWriter
{
    public class Insert<C> : BaseQuery<C> where C : class
    {
        private readonly C _item;

        internal Insert(C item, RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _sqlWriter = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _connection = connection;

            if (item == null) throw new SqlWriterException("Item must not be null.", new ArgumentException());

            _item = item;

            Statics.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped));
        }

        public Insert<C> Modify(QueryOptions queryOptions, ILogger? logger = null)
        {
            _queryOptions = queryOptions;

            _logger = logger;

            return this;
        }

        public override string ToString()
        {
            string sql = $"INSERT INTO \"{_registeredClass.DatabaseTableName}\" (";

            foreach (var property in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) sql = $"{sql}\"{property.ColumnName}\", ";

            sql = sql[0..^2];

            sql = $"{sql}) VALUES (";

            foreach (var property in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) sql = $"{sql}@w_{property.ColumnName}, ";

            sql = sql[0..^2];

            return $"{sql}) RETURNING *;";
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
