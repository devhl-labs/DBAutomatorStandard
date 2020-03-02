using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Dapper.SqlWriter.Interfaces;
using Dapper.SqlWriter.Models;
using MiaPlaza.ExpressionUtils;
using MiaPlaza.ExpressionUtils.Evaluating;
using System.Data;

namespace Dapper.SqlWriter 
{
    public class DeleteBase<C> : BaseQuery<C> where C : class
    {
        private List<ExpressionPart<C>> _whereExpressionParts = new List<ExpressionPart<C>>();

        private readonly C? _item = null;

        internal DeleteBase(RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _sqlWriter = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _connection = connection;
        }

        public DeleteBase<C> Options(QueryOptions queryOptions)
        {
            _queryOptions = queryOptions;

            return this;
        }

        internal DeleteBase(C item, RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _sqlWriter = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _connection = connection;

            _item = item ?? throw new SqlWriterException("Item must not be null.", new ArgumentException());

            if (_registeredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey))
            {
                Statics.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey));
            }
            else
            {
                Statics.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped));
            }
        }

        public DeleteBase<C> Where(Expression<Func<C, object>>? where)
        {
            where = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

            BinaryExpression binaryExpression = Statics.GetBinaryExpression(where);

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
                return GetSqlByItem();
            }
        }

        private string GetSqlByItem(bool allowSqlInjection = false)
        {
            if (_item == null) throw new SqlWriterException("Item must not be null.", new ArgumentException());

            string sql = $"DELETE FROM \"{_registeredClass.DatabaseTableName}\" WHERE";

            if (_registeredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey))
            {
                foreach (RegisteredProperty<C> prop in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey))
                {
                    if (allowSqlInjection && _item != null)
                    {
                        sql = $"{sql} {prop.ToSqlInjectionString(_item)} AND";
                    }
                    else
                    {
                        sql = $"{sql} {prop.ToString()} AND";
                    }
                }
            }
            else
            {
                foreach (var prop in _registeredClass.RegisteredProperties.Where(p => p.NotMapped))
                {
                    if (allowSqlInjection)
                    {
                        sql = $"{sql} {prop.ToSqlInjectionString(_item)} AND";
                    }
                    else
                    {
                        sql = $"{sql} {prop.ToString()} AND";
                    }
                }
            }

            if (sql.EndsWith(" AND")) sql = sql[..^4];

            return $"{sql} RETURNING *;";
        }

        private string GetSqlByExpression(bool allowSqlInjection = false)
        {
            string sql = $"DELETE FROM \"{_registeredClass.DatabaseTableName}\" ";

            if (_whereExpressionParts.Count > 0)
            {
                sql = $"{sql}WHERE ";

                //sql = $"{sql}{Statics.ToColumnNameEqualsParameterName(_registeredClass, _whereExpressionParts)} ";

                if (allowSqlInjection)
                {
                    foreach (var where in _whereExpressionParts) sql = $"{sql} {where.ToSqlInjectionString()}";
                }
                else
                {
                    foreach (var where in _whereExpressionParts) sql = $"{sql} {where.ToString()}";
                }                
            }

            return $"{sql} RETURNING *;";
        }

        public async Task<IEnumerable<C>> QueryAsync()
        {
            if (_item is IDBEvent dBEvent) _ = dBEvent.OnDeleteAsync(_sqlWriter);

            var result = await QueryAsync(QueryType.Delete, ToString()).ConfigureAwait(false);

            //if (_item is DBObject<C> dbObject && result is DBObject<C> resultDbObject)
            //{
            //    dbObject.ObjectState = resultDbObject.ObjectState;

            //    dbObject._oldValues = resultDbObject._oldValues;
            //}

            if (_item is IDBEvent dBEvent1) _ = dBEvent1.OnDeletedAsync(_sqlWriter);

            return result;
        }

        public async Task<List<C>> QueryToListAsync() => (await QueryAsync(QueryType.Delete, ToString()).ConfigureAwait(false)).ToList();
    }
}
