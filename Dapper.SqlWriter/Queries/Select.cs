using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class Select<C> : BaseQuery<C> where C : class
    {
        private readonly List<ExpressionPart> _selectExpressionParts = new List<ExpressionPart>();

        private int? _top = null;

        private int? _limit = null;

        private double? _topPercent = null;

        private Comparison _comparison;

        private int? _rowNum = null;

        private List<ExpressionPart> _whereExpressionParts = new List<ExpressionPart>();


        private List<ExpressionPart> _orderByExpressionParts = new List<ExpressionPart>();

        internal Select(Expression<Func<C, object>>? select, RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _sqlWriter = dBAutomator;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _connection = connection;

            if (select == null) return;

            select = PartialEvaluator.PartialEvalBody(select, ExpressionInterpreter.Instance);

            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(select);

            _selectExpressionParts = Statics.GetExpressionParts(binaryExpression);
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

            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(where);

            _whereExpressionParts = Statics.GetExpressionParts(binaryExpression);

            Statics.AddParameters(_p, _registeredClass, _whereExpressionParts);

            return this;
        }

        public Select<C> OrderBy(Expression<Func<C, object>> orderBy) => OrderBy(orderBy, true);

        public Select<C> OrderByDesc(Expression<Func<C, object>> orderBy) => OrderBy(orderBy, false);

        private Select<C> OrderBy(Expression<Func<C, object>> orderBy, bool ascending)
        {
            //orderBy = PartialEvaluator.PartialEvalBody(orderBy, ExpressionInterpreter.Instance);

            //BinaryExpression? binaryExpression = Statics.GetBinaryExpression(orderBy);

            //var parts = Statics.GetExpressionParts(binaryExpression);

            //_orderByExpressionParts ??= new List<ExpressionPart>();

            //foreach (var part in parts.EmptyIfNull())
            //{
            //    if (ascending) part.NodeType = ExpressionType.GreaterThan;

            //    if (!ascending) part.NodeType = ExpressionType.LessThan;

            //    _orderByExpressionParts.Add(part);
            //}

            //return this;

            orderBy = PartialEvaluator.PartialEvalBody(orderBy, ExpressionInterpreter.Instance);

            _orderByExpressionParts ??= new List<ExpressionPart>();

            ExpressionPart part = new ExpressionPart
            {
                MemberExpression = Statics.GetMemberExpression(orderBy)
            };

            if (ascending) part.NodeType = ExpressionType.GreaterThan;

            if (!ascending) part.NodeType = ExpressionType.LessThan;

            _orderByExpressionParts.Add(part);

            return this;
        }

        /// <summary>
        /// MySQL, Postgres, and others
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Select<C> Limit(int limit)
        {
            _limit = limit;

            return this;
        }

        /// <summary>
        /// SQL Server specific
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        public Select<C> Top(int top)
        {
            _top = top;

            return this;
        }

        /// <summary>
        /// SQL Server specific
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        public Select<C> TopPercent(int top)
        {
            _topPercent = top;

            return this;
        }

        /// <summary>
        /// Oracle specific
        /// </summary>
        /// <param name="comparison"></param>
        /// <param name="rowNum"></param>
        /// <returns></returns>
        public Select<C> RowNum(Comparison comparison, int rowNum)
        {
            _comparison = comparison;

            _rowNum = rowNum;

            return this;
        }

        public override string ToString()
        {
            string sql = $"SELECT";

            if (_top != null)
            {
                sql = $"{sql} TOP({_top})";
            }
            else if (_topPercent != null)
            {
                sql = $"{sql} TOP({_topPercent}) PERCENT";
            }

            if (_selectExpressionParts.Count == 0)
            {
                foreach (var property in _registeredClass.RegisteredProperties.Where(p => !p.NotMapped))
                {
                    sql = $"{sql} \"{property.ColumnName}\",";
                }
            }
            else
            {
                foreach(var expression in _selectExpressionParts.Where(e => e.MemberExpression != null))
                {
                    RegisteredProperty<C> registeredProperty = Statics.GetRegisteredProperty(_registeredClass, expression.MemberExpression!);

                    sql = $"{sql} \"{registeredProperty.ColumnName}\", ";
                }

                sql = sql[..^1];
            }

            sql = sql[0..^1];

            sql = $"{sql} FROM \"{_registeredClass.DatabaseTableName}\"";

            if (_whereExpressionParts.Count > 0)
            {
                sql = $"{sql} WHERE";

                if (_rowNum != null) sql = $"{sql} ROWNUM {_comparison.GetOperator()} {_rowNum}";

                sql = $"{sql}{Statics.ToColumnNameEqualsParameterName(_registeredClass, _whereExpressionParts)}";
            }

            if (_orderByExpressionParts.Count > 0)
            {
                sql = $"{sql} ORDER BY";

                foreach(var expressionPart in _orderByExpressionParts)
                {
                    RegisteredProperty<C> registeredProperty = _registeredClass.RegisteredProperties.First(p => p.PropertyName == expressionPart.MemberExpression?.Member.Name);

                    if (expressionPart.NodeType == ExpressionType.GreaterThan) sql = $"{sql} \"{registeredProperty.ColumnName}\" ASC";

                    if (expressionPart.NodeType == ExpressionType.LessThan) sql = $"{sql} \"{registeredProperty.ColumnName}\" DESC";
                }
            }

            if (_limit != null) sql = $"{sql} LIMIT {_limit}";

            return $"{sql};";
        }

        public async Task<IEnumerable<C>> QueryAsync() => await QueryAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<C> QueryFirstAsync() => await QueryFirstAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<C?> QueryFirstOrDefaultAsync() => await QueryFirstOrDefaultAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<C> QuerySingleAsync() => await QuerySingleAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<C?> QuerySingleOrDefaultAsync() => await QuerySingleOrDefaultAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<List<C>> QueryToListAsync() => (await QueryAsync(QueryType.Select, ToString()).ConfigureAwait(false)).ToList();
    }
}
