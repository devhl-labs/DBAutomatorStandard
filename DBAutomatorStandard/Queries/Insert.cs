using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using devhl.DBAutomator.Interfaces;
using System.Data;

namespace devhl.DBAutomator
{
    public class Insert<C> : BaseQuery<C>
    {
        private readonly C _item;

        internal Insert(C item, RegisteredClass<C> registeredClass, DBAutomator dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _connection = connection;

            if (item == null) throw new DbAutomatorException("Item must not be null.", new ArgumentException());

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
            string sql = $"INSERT INTO \"{_registeredClass.TableName}\" (";

            foreach (var property in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) sql = $"{sql}\"{property.ColumnName}\", ";

            sql = sql[0..^2];

            sql = $"{sql}) VALUES (";

            foreach (var property in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) sql = $"{sql}@w_{property.ColumnName}, ";

            sql = sql[0..^2];

            return $"{sql}) RETURNING *;";
        }

        public async Task<C> QueryFirstOrDefaultAsync() => await QueryFirstOrDefaultAsync(ToString()).ConfigureAwait(false);
    }
}
