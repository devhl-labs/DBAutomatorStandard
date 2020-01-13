using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Dapper;
using Npgsql;

using devhl.DBAutomator.Interfaces;
using devhl.DBAutomator.Models;
using devhl.Common;
using MiaPlaza.ExpressionUtils;
using MiaPlaza.ExpressionUtils.Evaluating;

namespace devhl.DBAutomator
{
    public class Select<C> : BasePostgresQuery<C>
    {
        private readonly List<ExpressionPart> _selectExpressionParts = new List<ExpressionPart>();


        private List<ExpressionPart> _whereExpressionParts = new List<ExpressionPart>();


        private List<ExpressionPart> _orderByExpressionParts = new List<ExpressionPart>();

        internal Select(Expression<Func<C, object>>? select, RegisteredClass<C> registeredClass, DBAutomator dBAutomator, QueryOptions queryOptions, ILogger? logger = null)
        {
            _dBAutomator = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            if (select == null) return;

            select = PartialEvaluator.PartialEvalBody(select, ExpressionInterpreter.Instance);

            BinaryExpression? binaryExpression = PostgresMethods.GetBinaryExpression(select);

            _selectExpressionParts = PostgresMethods.GetExpressionParts(binaryExpression);
        }

        public Select<C> Modify(QueryOptions queryOptions, ILogger? logger = null)
        {
            _queryOptions = queryOptions;

            _logger = logger;

            return this;
        }

        public Select<C> Where(Expression<Func<C, object>> where)
        {
            where = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

            BinaryExpression? binaryExpression = PostgresMethods.GetBinaryExpression(where);

            _whereExpressionParts = PostgresMethods.GetExpressionParts(binaryExpression);

            PostgresMethods.AddParameters(_p, _registeredClass, _whereExpressionParts);

            return this;
        }

        public Select<C> OrderBy(Expression<Func<C, object>> orderBy) => OrderBy(orderBy, true);

        public Select<C> OrderByDesc(Expression<Func<C, object>> orderBy) => OrderBy(orderBy, false);

        private Select<C> OrderBy(Expression<Func<C, object>> orderBy, bool ascending)
        {
            orderBy = PartialEvaluator.PartialEvalBody(orderBy, ExpressionInterpreter.Instance);

            BinaryExpression? binaryExpression = PostgresMethods.GetBinaryExpression(orderBy);

            var parts = PostgresMethods.GetExpressionParts(binaryExpression);

            _orderByExpressionParts ??= new List<ExpressionPart>();

            foreach(var part in parts.EmptyIfNull())
            {
                if (ascending) part.NodeType = ExpressionType.GreaterThan;

                if (!ascending) part.NodeType = ExpressionType.LessThan;                

                _orderByExpressionParts.Add(part);
            }

            return this;
        }

        public override string ToString()
        {
            string sql = $"SELECT";

            if (_selectExpressionParts.Count == 0)
            {
                foreach (var property in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped))
                {
                    sql = $"{sql} \"{property.ColumnName}\",";
                }
            }
            else
            {
                sql = $"{sql} {PostgresMethods.ToColumnNameEqualsParameterName(_registeredClass, _selectExpressionParts)}";
            }

            sql = sql[0..^1];

            sql = $"{sql} FROM \"{_registeredClass.TableName}\"";

            if (_whereExpressionParts.Count > 0)
            {
                sql = $"{sql} WHERE";

                sql = $"{sql}{PostgresMethods.ToColumnNameEqualsParameterName(_registeredClass, _whereExpressionParts)}";
            }

            if (_orderByExpressionParts.Count > 0)
            {
                foreach(var expressionPart in _orderByExpressionParts)
                {
                    RegisteredProperty registeredProperty = _registeredClass.RegisteredProperties.First(p => p.PropertyName == expressionPart.MemberExpression?.Member.Name);

                    if (expressionPart.NodeType == ExpressionType.GreaterThan) sql = $"{sql} \"{registeredProperty.ColumnName}\" ASC";

                    if (expressionPart.NodeType == ExpressionType.LessThan) sql = $"{sql} \"{registeredProperty.ColumnName}\" DESC";
                }
            }

            return $"{sql};";
        }

        public async Task<IEnumerable<C>> QueryAsync() => await QueryAsync(ToString()).ConfigureAwait(false);

        public async Task<C> QueryFirstAsync() => await QueryFirstAsync(ToString()).ConfigureAwait(false);

        public async Task<C> QueryFirstOrDefaultAsync() => await QueryFirstOrDefaultAsync(ToString()).ConfigureAwait(false);

        public async Task<C> QuerySingleAsync() => await QuerySingleAsync(ToString()).ConfigureAwait(false);

        public async Task<C> QuerySingleOrDefaultAsync() => await QuerySingleOrDefaultAsync(ToString()).ConfigureAwait(false);
    }
}
