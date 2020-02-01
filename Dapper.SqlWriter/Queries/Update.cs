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
    public class Update<C> : BaseQuery<C> where C : class
    {
        private readonly C? _item = null;


        private List<ExpressionPart> _setExpressionParts = new List<ExpressionPart>();
        

        private List<ExpressionPart> _whereExpressionParts = new List<ExpressionPart>();

        internal Update(C item, RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;

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

        internal Update(RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _connection = connection;
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

            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(set);

            _setExpressionParts = Statics.GetExpressionParts(binaryExpression);

            Statics.AddParameters(_p, _registeredClass, _setExpressionParts, "s_");

            return this;
        }

        public Update<C> Where(Expression<Func<C, object>> where)
        {
            where = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(where);

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
            if (_item == null) throw new SqlWriterException("The item cannot be null.", new NullReferenceException());

            string sql = $"UPDATE \"{_registeredClass.TableName}\" SET {Statics.ToColumnNameEqualsParameterName(_registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement), "s_", ", ")} WHERE";

            if (_whereExpressionParts?.Count > 0)
            {
                sql = $"{sql} {Statics.ToColumnNameEqualsParameterName(_registeredClass, _whereExpressionParts)}";
            }
            else
            {
                if (_registeredClass.RegisteredProperties.Count(p => !p.NotMapped && p.IsKey) == 0) throw new SqlWriterException("The item does not have a key registered nor a where clause.", new ArgumentException());

                sql = $"{sql} {Statics.ToColumnNameEqualsParameterName(_registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey), delimiter: " AND")}";
            }

            return $"{sql} RETURNING *;";
        }

        public async Task<C> QueryFirstOrDefaultAsync() => await QueryFirstOrDefaultAsync(ToString());

        public async Task<List<C>> QueryToListAsync() => (await QueryAsync(ToString()).ConfigureAwait(false)).ToList();

        private string GetSqlByExpression()
        {
            string sql = $"UPDATE \"{_registeredClass.TableName}\" SET {Statics.ToColumnNameEqualsParameterName(_registeredClass, _setExpressionParts, "s_")}";

            if (_whereExpressionParts.Count > 0)
            {
                sql = $"{sql} WHERE {Statics.ToColumnNameEqualsParameterName(_registeredClass, _whereExpressionParts, "w_")}";
            }

            return $"{sql} RETURNING *;";
        }
    }
}
