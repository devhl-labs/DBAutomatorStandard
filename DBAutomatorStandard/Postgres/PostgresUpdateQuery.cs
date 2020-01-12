using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using devhl.DBAutomator.Interfaces;
using devhl.DBAutomator.Models;
using MiaPlaza.ExpressionUtils.Evaluating;
using MiaPlaza.ExpressionUtils;

namespace devhl.DBAutomator
{
    public class Update<C> : BasePostgresQuery<C> where C : class
    {
        private readonly C? _item = null;


        private List<ExpressionPart> _setExpressionParts = new List<ExpressionPart>();
        

        private List<ExpressionPart> _whereExpressionParts = new List<ExpressionPart>();

        internal Update(C item, RegisteredClass<C> registeredClass, DBAutomator dBAutomator, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _item = item;

            PostgresMethods.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement), "s_");

            PostgresMethods.AddParameters(_p, _item, _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement && p.IsKey));
        }

        internal Update(RegisteredClass<C> registeredClass, DBAutomator dBAutomator, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;
        }

        public Update<C> Modify(QueryOptions queryOptions, ILogger? logger = null)
        {
            _queryOptions = queryOptions;

            _logger = logger;

            return this;
        }

        public Update<C> Set(Expression<Func<C, object>> set)
        {
            set = PartialEvaluator.PartialEvalBody(set, ExpressionInterpreter.Instance);

            BinaryExpression? binaryExpression = PostgresMethods.GetBinaryExpression(set);

            _setExpressionParts = PostgresMethods.GetExpressionParts(binaryExpression);

            PostgresMethods.AddParameters(_p, _registeredClass, _setExpressionParts, "s_");

            return this;
        }

        public Update<C> Where(Expression<Func<C, object>> where)
        {
            where = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

            BinaryExpression? binaryExpression = PostgresMethods.GetBinaryExpression(where);

            _whereExpressionParts = PostgresMethods.GetExpressionParts(binaryExpression);

            PostgresMethods.AddParameters(_p, _registeredClass, _whereExpressionParts);

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
            if (_item == null) throw new DbAutomatorException("The item cannot be null.", new NullReferenceException());

            string sql = $"UPDATE \"{_registeredClass.TableName}\" SET {PostgresMethods.ToColumnNameEqualsParameterName(_registeredClass.RegisteredProperties.Where(p => !p.NotMapped), "s_")} WHERE";

            if (_whereExpressionParts?.Count > 0)
            {
                sql = $"{sql} {PostgresMethods.ToColumnNameEqualsParameterName(_registeredClass, _whereExpressionParts)}";
            }
            else
            {
                if (_registeredClass.RegisteredProperties.Count(p => !p.NotMapped && p.IsKey) == 0) throw new DbAutomatorException("The item does not have a key registered nor a where clause.", new ArgumentException());

                foreach(var key in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey))
                {
                    sql = $"{sql} {PostgresMethods.ToColumnNameEqualsParameterName(_registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey))}";
                }
            }

            return $"{sql} RETURNING *;";
        }

        public async Task<C> QueryFirstOrDefaultAsync() => await QueryFirstOrDefaultAsync(ToString());

        private string GetSqlByExpression()
        {
            string sql = $"UPDATE \"{_registeredClass.TableName}\" SET {PostgresMethods.ToColumnNameEqualsParameterName(_registeredClass, _setExpressionParts, "s_")}";

            if (_whereExpressionParts.Count > 0)
            {
                sql = $"{sql} WHERE {PostgresMethods.ToColumnNameEqualsParameterName(_registeredClass, _whereExpressionParts, "w_")}";
            }

            return $"{sql} RETURNING *;";
        }
    }
}
