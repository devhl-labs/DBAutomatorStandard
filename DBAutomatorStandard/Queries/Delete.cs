using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using devhl.DBAutomator.Interfaces;
using devhl.DBAutomator.Models;
using MiaPlaza.ExpressionUtils;
using MiaPlaza.ExpressionUtils.Evaluating;
using System.Data;

namespace devhl.DBAutomator 
{
    public class Delete<C> : BaseQuery<C> where C : class
    {
        private List<ExpressionPart> _whereExpressionParts = new List<ExpressionPart>();

        private readonly C? _item = null;

        internal Delete(RegisteredClass<C> registeredClass, DBAutomator dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _connection = connection;
        }

        public Delete<C> Modify(QueryOptions queryOptions, ILogger? logger = null)
        {
            _queryOptions = queryOptions;

            _logger = logger;

            return this;
        }

        internal Delete(C item, RegisteredClass<C> registeredClass, DBAutomator dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _connection = connection;

            _item = item ?? throw new DbAutomatorException("Item must not be null.", new ArgumentException());

            if (_registeredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey))
            {
                Statics.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement && p.IsKey));
            }
            else
            {
                Statics.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped));
            }

        }

        public Delete<C> Where(Expression<Func<C, object>>? where)
        {
            where = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

            BinaryExpression binaryExpression = Statics.GetBinaryExpression(where);

            _whereExpressionParts = Statics.GetExpressionParts(binaryExpression);

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

        private string GetSqlByItem()
        {
            string sql = $"DELETE FROM \"{_registeredClass.TableName}\" WHERE";

            if (_registeredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey))
            {
                sql = $"{sql} {Statics.ToColumnNameEqualsParameterName(_registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey))}";
            }
            else
            {
                sql = $"{sql} {Statics.ToColumnNameEqualsParameterName(_registeredClass.RegisteredProperties.Where(p => !p.NotMapped))}";
            }

            return $"{sql} RETURNING *;";
        }

        private string GetSqlByExpression()
        {
            string sql = $"DELETE FROM \"{_registeredClass.TableName}\" ";

            if (_whereExpressionParts.Count > 0)
            {
                sql = $"{sql}WHERE ";

                sql = $"{sql}{Statics.ToColumnNameEqualsParameterName(_registeredClass, _whereExpressionParts)} ";
            }

            return $"{sql}RETURNING *;";
        }

        public async Task<IEnumerable<C>> QueryAsync() => await QueryAsync(ToString()).ConfigureAwait(false);
    }
}
