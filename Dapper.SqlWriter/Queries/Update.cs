using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Dapper.SqlWriter.Interfaces;
using Dapper.SqlWriter.Models;
using MiaPlaza.ExpressionUtils.Evaluating;
using MiaPlaza.ExpressionUtils;

namespace Dapper.SqlWriter
{
    public class UpdateBase<C> : BaseQuery<C> where C : class
    {
        private readonly C? _item = null;


        private List<ExpressionPart<C>> _setExpressionParts = new List<ExpressionPart<C>>();
        

        private List<ExpressionPart<C>> _whereExpressionParts = new List<ExpressionPart<C>>();

        internal UpdateBase(C item, RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _sqlWriter = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _item = item;

            _connection = connection;

            Statics.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement), "s_");

            Statics.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement && p.IsKey));
            
            if (_registeredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey && p.IsAutoIncrement))
            {
                Statics.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey && p.IsAutoIncrement));
            }
            else
            {
                Statics.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped));
            }
        }

        internal UpdateBase(RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _sqlWriter = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _connection = connection;
        }

        public UpdateBase<C> Options(QueryOptions queryOptions)
        {
            _queryOptions = queryOptions;

            return this;
        }

        public UpdateBase<C> Set(Expression<Func<C, object>> set)
        {
            if (_item != null) throw new SqlWriterException("This method does not support instantiated objects.", new ArgumentException());

            set = set.RemoveClosure()!;

            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(set);

            _setExpressionParts = Statics.GetExpressionParts(binaryExpression, _registeredClass, null, "s_");

            Statics.AddParameters(_p, _registeredClass, _setExpressionParts, "s_");

            return this;
        }

        public UpdateBase<C> Where(Expression<Func<C, object>> where)
        {
            if (_item != null) throw new SqlWriterException("This method does not support instantiated objects.", new ArgumentException());

            where = where.RemoveClosure()!;

            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(where);

            _whereExpressionParts = Statics.GetExpressionParts(binaryExpression, _registeredClass);

            Statics.AddParameters(_p, _registeredClass, _whereExpressionParts);

            return this;
        }

        public override string ToString()
        {
            if (_item == null)
            {
                return GetSqlByExpression();
            }
            else
            {
                return GetSqlByItem();
            }
        }

        public string ToSqlInjectionString()
        {
            if (_item == null)
            {
                return GetSqlByExpression(true);
            }
            else
            {
                return GetSqlByItem(true);
            }
        }

        private string GetSqlByItem(bool allowSqlInjection = false)
        {
            if (_item == null) throw new SqlWriterException("Item must not be null.", new NullReferenceException());

            //string sql = $"UPDATE \"{_registeredClass.DatabaseTableName}\" SET {Statics.ToColumnNameEqualsParameterName(_registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement), "s_", ", ")} WHERE";

            string sql = $"UPDATE \"{_registeredClass.DatabaseTableName}\" SET ";

            if (allowSqlInjection)
            {
                foreach (var prop in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) sql = $"{sql} {prop.ToSqlInjectionString(_item)}, ";
            }
            else
            {
                foreach (var prop in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) sql = $"{sql} {prop.ToString("s_")}, ";
            }

            if (sql.EndsWith(", ")) sql = sql[..^2];

            sql = $"{sql} WHERE";

            if (_whereExpressionParts?.Count > 0)
            {
                //sql = $"{sql} {Statics.ToColumnNameEqualsParameterName(_registeredClass, _whereExpressionParts)}";

                if (allowSqlInjection)
                {
                    foreach (var where in _whereExpressionParts) sql = $"{sql} {where.ToSqlInjectionString()}";
                }
                else
                {
                    foreach (var where in _whereExpressionParts) sql = $"{sql} {where.ToString()}";
                }

            }
            else
            {
                if (_registeredClass.RegisteredProperties.Count(p => !p.NotMapped && p.IsKey) == 0) throw new SqlWriterException("The item does not have a key registered nor a where clause.", new ArgumentException());

                //sql = $"{sql} {Statics.ToColumnNameEqualsParameterName(_registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey), delimiter: " AND")}";

                if (allowSqlInjection)
                {
                    foreach (var prop in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey)) sql = $"{sql} {prop.ToSqlInjectionString(_item)} AND";
                }
                else
                {
                    foreach (var prop in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey)) sql = $"{sql} {prop.ToString()} AND";
                }
            }

            if (sql.EndsWith("AND")) sql = sql[..^3];

            return $"{sql} RETURNING *;";
        }

        public async Task<C> QueryFirstAsync()
        {
            if (_item is IDBEvent dBEvent) _ = dBEvent.OnUpdateAsync(_sqlWriter);

            var result = await QueryFirstAsync(QueryType.Update, ToString());

            if (_item is IDBEvent dBEvent1) _ = dBEvent1.OnUpdatedAsync(_sqlWriter);

            return result;
        }

        public async Task<List<C>> QueryToListAsync() => (await QueryAsync(QueryType.Update, ToString()).ConfigureAwait(false)).ToList();

        public async Task<List<T>> QueryToListAsync<T>()
        {
            IEnumerable<C> records = await QueryAsync(QueryType.Update, ToString()).ConfigureAwait(false);

            List<T> results = new List<T>();

            foreach (C record in records)
            {
                object recordObject = record;

                results.Add((T) recordObject);
            }

            return results;
        }

        private string GetSqlByExpression(bool allowSqlInjection = false)
        {
            string sql = $"UPDATE \"{_registeredClass.DatabaseTableName}\" SET";

            if (allowSqlInjection)
            {
                foreach (var set in _setExpressionParts) sql = $"{sql} {set.ToSqlInjectionString()}";
            }
            else
            {
                foreach (ExpressionPart<C> set in _setExpressionParts.Where(s => s.ConstantExpression != null && s.MemberExpression != null)) 
                    sql = $"{sql}{set.GetSetString()}, ";
            }

            if (sql.EndsWith(", "))
                sql = sql[0..^2];

            if (_whereExpressionParts.Count > 0)
            {
                sql = $"{sql} WHERE";

                if (allowSqlInjection)
                {
                    foreach (var where in _whereExpressionParts) sql = $"{sql} {where.ToSqlInjectionString()}";
                }
                else
                {
                    foreach (var where in _whereExpressionParts) sql = $"{sql} {where}";
                }
            }

            return $"{sql} RETURNING *;";
        }
    }
}
