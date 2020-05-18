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
using System.ComponentModel;

namespace Dapper.SqlWriter
{
    public sealed class Select<C> : BaseQuery<C> where C : class
    {
        private IEnumerable<C>? Results { get; set; } = null;

        private List<ExpressionPart<C>> SelectExpressionParts { get; } = new List<ExpressionPart<C>>();

        private int? TopValue { get; set; } = null;

        private int? LimitValue { get; set; } = null;

        private double? TopPercentValue { get; set; } = null;

        private Comparison Comparison { get; set; }

        private int? RowNumValue { get; set; } = null;

        private List<ExpressionPart<C>> WhereExpressionParts { get; set; } = new List<ExpressionPart<C>>();

        private readonly List<ExpressionPart<C>> _orWhereExpressionParts = new List<ExpressionPart<C>>();

        private string TableNameValue { get; set; } = string.Empty;

        private bool IsDistinct { get; set; }

        private List<ExpressionPart<C>> _orderByExpressionParts = new List<ExpressionPart<C>>();

        internal Select(RegisteredClass<C> registeredClass, SqlWriter sqlWriter, IDbConnection connection, QueryOptions queryOptions, ILogger? logger = null)
        {
            SqlWriter = sqlWriter;

            QueryOptions = queryOptions;

            Logger = logger;

            RegisteredClass = registeredClass;

            TableNameValue = RegisteredClass.DatabaseTableName;

            Connection = connection;
        }

        public Select<C> TableName(string tableName)
        {
            TableNameValue = tableName;

            return this;
        }

        public Select<C> Options(QueryOptions queryOptions)
        {
            QueryOptions = queryOptions;

            return this;
        }

        public Select<C> Column(Expression<Func<C, object>> column)
        {
            column = column.RemoveClosure();

            ExpressionPart<C> part = new ExpressionPart<C>
            {
                MemberExpression = Statics.GetMemberExpression(column)
            };

            SelectExpressionParts.Add(part);

            return this;
        }

        public Select<C> Distinct()
        {
            IsDistinct = true;

            return this;
        }

        public Select<C> Where(Expression<Func<C, object>> where)
        {
            where = where.RemoveClosure();

            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(where);

            WhereExpressionParts = Statics.GetExpressionParts(binaryExpression, RegisteredClass);

            Statics.AddParameters(P, RegisteredClass, WhereExpressionParts);

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

            _orWhereExpressionParts.AddRange(Statics.GetExpressionParts(binaryExpression, RegisteredClass, null, "orw_"));

            Statics.AddParameters(P, RegisteredClass, _orWhereExpressionParts, "orw_");

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
            LimitValue = limit;

            return this;
        }

        /// <summary>
        /// SQL Server specific
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        public Select<C> Top(int top)
        {
            TopValue = top;

            return this;
        }

        /// <summary>
        /// SQL Server specific
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        public Select<C> TopPercent(int top)
        {
            TopPercentValue = top;

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
            Comparison = comparison;

            RowNumValue = rowNum;

            return this;
        }

        public string ToSqlInjectionString() => GetString(true);

        public override string ToString() => GetString();

        private string GetString(bool allowSqlInjection = false)
        {
            string sql = $"SELECT";

            if (IsDistinct)
            {
                sql = $"{sql} DISTINCT";
            }

            if (TopValue != null)
            {
                sql = $"{sql} TOP({TopValue})";
            }
            else if (TopPercentValue != null)
            {
                sql = $"{sql} TOP({TopPercentValue}) PERCENT";
            }

            if (SelectExpressionParts.Count == 0)
            {
                foreach (var property in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped))
                {
                    sql = $"{sql} \"{property.ColumnName}\",";
                }
            }
            else
            {
                foreach (var expression in SelectExpressionParts.Where(e => e.MemberExpression != null))
                {
                    RegisteredProperty<C> registeredProperty = Statics.GetRegisteredProperty(RegisteredClass, expression.MemberExpression!);

                    sql = $"{sql} \"{registeredProperty.ColumnName}\", ";
                }

                sql = sql[..^1];
            }

            sql = sql[0..^1];

            sql = $"{sql} FROM \"{TableNameValue}\"";

            if (WhereExpressionParts.Count > 0 || _orWhereExpressionParts.Count > 0)
            {
                sql = $"{sql} WHERE";
            }

            if (WhereExpressionParts.Count > 0)
            {
                if (RowNumValue != null) sql = $"{sql} ROWNUM {Comparison.GetOperator()} {RowNumValue}";

                if (allowSqlInjection)
                {
                    foreach (var where in WhereExpressionParts) sql = $"{sql} {where.ToSqlInjectionString()}";
                }
                else
                {
                    foreach (var where in WhereExpressionParts) sql = $"{sql} {where}";
                }
            }

            if (_orWhereExpressionParts.Count > 0)
            {
                if (WhereExpressionParts.Count > 0)
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

                if (WhereExpressionParts.Count > 0)
                    sql = $"{sql})";
            }

            if (_orderByExpressionParts.Count > 0)
            {
                sql = $"{sql} ORDER BY";

                foreach (var expressionPart in _orderByExpressionParts)
                {
                    RegisteredProperty<C> registeredProperty = RegisteredClass.RegisteredProperties.First(p => p.PropertyName == expressionPart.MemberExpression?.Member.Name);

                    if (expressionPart.NodeType == ExpressionType.GreaterThan) sql = $"{sql} \"{registeredProperty.ColumnName}\" ASC";

                    if (expressionPart.NodeType == ExpressionType.LessThan) sql = $"{sql} \"{registeredProperty.ColumnName}\" DESC";
                }
            }

            if (LimitValue != null) sql = $"{sql} LIMIT {LimitValue}";

            return $"{sql};";
        }

        public async Task<IEnumerable<C>> QueryAsync() => await QueryAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<C> QueryFirstAsync() => await QueryFirstAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<C?> QueryFirstOrDefaultAsync() => await QueryFirstOrDefaultAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<C> QuerySingleAsync() => await QuerySingleAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<C?> QuerySingleOrDefaultAsync() => await QuerySingleOrDefaultAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<List<C>> QueryToListAsync() => (await QueryAsync(QueryType.Select, ToString()).ConfigureAwait(false)).ToList();

        public async Task<List<T>> QueryToListAsync<T>() where T : class
        {
            Results = await QueryAsync(QueryType.Select, ToString()).ConfigureAwait(false);

            List<T> results = new List<T>();

            foreach (var result in Results)
            {
                object recordObject = result;

                T item = (T) recordObject;

                results.Add(item);
            }

            return results;
        }
    }
}
