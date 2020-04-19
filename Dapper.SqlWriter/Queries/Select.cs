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
        private readonly List<ExpressionPart<C>> _selectExpressionParts = new List<ExpressionPart<C>>();

        private int? _top = null;

        private int? _limit = null;

        private double? _topPercent = null;

        private Comparison _comparison;

        private int? _rowNum = null;

        private List<ExpressionPart<C>> _whereExpressionParts = new List<ExpressionPart<C>>();

        private readonly List<ExpressionPart<C>> _orWhereExpressionParts = new List<ExpressionPart<C>>();

        //private readonly List<ExpressionPart<C>> _andWhereExpressionParts = new List<ExpressionPart<C>>();

        private string _tableName = string.Empty;

        private bool _isDistint;


        private List<ExpressionPart<C>> _orderByExpressionParts = new List<ExpressionPart<C>>();

        internal Select(RegisteredClass<C> registeredClass, SqlWriter sqlWriter, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            _sqlWriter = sqlWriter;

            _queryOptions = queryOptions;

            _logger = logger;

            _registeredClass = registeredClass;

            _tableName = _registeredClass.DatabaseTableName;

            _connection = connection;
        }

        public Select<C> TableName(string tableName)
        {
            _tableName = tableName;

            return this;
        }

        public Select<C> Options(QueryOptions queryOptions)
        {
            _queryOptions = queryOptions;

            return this;
        }

        public Select<C> Column(Expression<Func<C, object>> column)
        {
            column = column.RemoveClosure();

            ExpressionPart<C> part = new ExpressionPart<C>
            {
                MemberExpression = Statics.GetMemberExpression(column)
            };

            _selectExpressionParts.Add(part);

            return this;
        }

        public Select<C> Distinct()
        {
            _isDistint = true;

            return this;
        }

        public Select<C> Where(Expression<Func<C, object>> where)
        {
            where = where.RemoveClosure();

            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(where);

            _whereExpressionParts = Statics.GetExpressionParts(binaryExpression, _registeredClass);

            Statics.AddParameters(_p, _registeredClass, _whereExpressionParts);

            return this;
        }

        /// <summary>
        /// Use this when building the sql while iterating a collection.  It will result in ...Where() AND ( WhereOr[0] OR WhereOr[1]...);
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public Select<C> OrWhere(Expression<Func<C, object>> where)
        {
            where = where.RemoveClosure()!;

            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(where);

            _orWhereExpressionParts.Add(new ExpressionPart<C> { Parens = Parens.Left});

            _orWhereExpressionParts.AddRange(Statics.GetExpressionParts(binaryExpression, _registeredClass, null, "orw_"));

            Statics.AddParameters(_p, _registeredClass, _orWhereExpressionParts, "orw_");

            _orWhereExpressionParts.Add(new ExpressionPart<C> { Parens = Parens.Right });

            _orWhereExpressionParts.Add(new ExpressionPart<C> { NodeType = ExpressionType.OrElse});

            return this;
        }

        //public Select<C> AndWhere(Expression<Func<C, object>> where)
        //{
        //    where = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

        //    BinaryExpression? binaryExpression = Statics.GetBinaryExpression(where);

        //    _andWhereExpressionParts.AddRange(Statics.GetExpressionParts(binaryExpression, _registeredClass));

        //    Statics.AddParameters(_p, _registeredClass, _orWhereExpressionParts);

        //    return this;
        //}

        public Select<C> OrderBy(Expression<Func<C, object>> orderBy) => OrderBy(orderBy, true);

        public Select<C> OrderByDesc(Expression<Func<C, object>> orderBy) => OrderBy(orderBy, false);

        private Select<C> OrderBy(Expression<Func<C, object>> orderBy, bool ascending)
        {
            orderBy = orderBy.RemoveClosure();

            _orderByExpressionParts ??= new List<ExpressionPart<C>>();

            ExpressionPart<C> part = new ExpressionPart<C>
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

        public string ToSqlInjectionString() => GetString(true);

        public override string ToString() => GetString();

        private string GetString(bool allowSqlInjection = false)
        {
            string sql = $"SELECT";

            if (_isDistint)
            {
                sql = $"{sql} DISTINCT";
            }

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
                foreach (var expression in _selectExpressionParts.Where(e => e.MemberExpression != null))
                {
                    RegisteredProperty<C> registeredProperty = Statics.GetRegisteredProperty(_registeredClass, expression.MemberExpression!);

                    sql = $"{sql} \"{registeredProperty.ColumnName}\", ";
                }

                sql = sql[..^1];
            }

            sql = sql[0..^1];

            sql = $"{sql} FROM \"{_tableName}\"";

            if (_whereExpressionParts.Count > 0 || _orWhereExpressionParts.Count > 0)
            {
                sql = $"{sql} WHERE";
            }

            if (_whereExpressionParts.Count > 0)
            {
                if (_rowNum != null) sql = $"{sql} ROWNUM {_comparison.GetOperator()} {_rowNum}";

                if (allowSqlInjection)
                {
                    foreach (var where in _whereExpressionParts) sql = $"{sql} {where.ToSqlInjectionString()}";
                }
                else
                {
                    foreach (var where in _whereExpressionParts) sql = $"{sql} {where}";
                }
            }

            if (_orWhereExpressionParts.Count > 0)
            {
                if (_whereExpressionParts.Count > 0)
                    sql = $"{sql} AND (";

                if (allowSqlInjection)
                {
                    foreach (var where in _orWhereExpressionParts)
                        sql = $"{sql} {where.ToSqlInjectionString()}";

                }
                else
                {
                    foreach (var where in _orWhereExpressionParts)
                        sql = $"{sql} {where}";
                }

                sql = sql[..^2];

                if (_whereExpressionParts.Count > 0)
                    sql = $"{sql})";
            }

            if (_orderByExpressionParts.Count > 0)
            {
                sql = $"{sql} ORDER BY";

                foreach (var expressionPart in _orderByExpressionParts)
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

        public async Task<List<T>> QueryToListAsync<T>()
        {
            IEnumerable<C> records = await QueryAsync(QueryType.Select, ToString()).ConfigureAwait(false);

            List<T> results = new List<T>();

            foreach (C record in records)
            {
                object recordObject = record;

                results.Add((T) recordObject);
            }

            return results;
        }
    }
}
