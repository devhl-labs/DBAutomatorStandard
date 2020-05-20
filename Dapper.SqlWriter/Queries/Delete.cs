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
    public sealed class Delete<C> : BaseQuery<C> where C : class
    {
        private List<ExpressionPart<C>> WhereExpressionParts { get; set; } = new List<ExpressionPart<C>>();

        private C? Item { get; set; }

        internal Delete(RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, SqlWriterConfiguration queryOptions, ILogger? logger = null)
        {
            SqlWriter = dBAutomator;

            QueryOptions = queryOptions;

            Logger = logger;

            RegisteredClass = registeredClass;

            Connection = connection;
        }

        public Delete<C> Options(SqlWriterConfiguration queryOptions)
        {
            QueryOptions = queryOptions;

            return this;
        }

        internal Delete(C item, RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, SqlWriterConfiguration queryOptions, ILogger? logger = null)
        {
            SqlWriter = dBAutomator;

            QueryOptions = queryOptions;

            Logger = logger;

            RegisteredClass = registeredClass;

            Connection = connection;

            Item = item ?? throw new SqlWriterException("Item must not be null.", new ArgumentException());

            if (RegisteredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey))
            {
                Statics.AddParameters(P, Item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey));
            }
            else
            {
                Statics.AddParameters(P, Item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped));
            }
        }

        public Delete<C> Where(Expression<Func<C, object>>? where)
        {
            where = where.RemoveClosure();

            BinaryExpression binaryExpression = Statics.GetBinaryExpression(where);

            WhereExpressionParts = Statics.GetExpressionParts(binaryExpression, RegisteredClass);

            Statics.AddParameters(P, RegisteredClass, WhereExpressionParts);

            return this;
        }

        public override string ToString()
        {
            if (Item == null)
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
            if (Item == null)
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
            if (Item == null) throw new SqlWriterException("Item must not be null.", new ArgumentException());

            string sql = $"DELETE FROM \"{RegisteredClass.DatabaseTableName}\" WHERE";

            if (RegisteredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey))
            {
                foreach (RegisteredProperty<C> prop in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey))
                {
                    if (allowSqlInjection && Item != null)
                    {
                        sql = $"{sql} {prop.ToSqlInjectionString(Item)} AND";
                    }
                    else
                    {
                        sql = $"{sql} {prop} AND";
                    }
                }
            }
            else
            {
                foreach (var prop in RegisteredClass.RegisteredProperties.Where(p => p.NotMapped))
                {
                    if (allowSqlInjection)
                    {
                        sql = $"{sql} {prop.ToSqlInjectionString(Item)} AND";
                    }
                    else
                    {
                        sql = $"{sql} {prop} AND";
                    }
                }
            }

            if (sql.EndsWith(" AND")) sql = sql[..^4];

            return $"{sql} RETURNING *;";
        }

        private string GetSqlByExpression(bool allowSqlInjection = false)
        {
            string sql = $"DELETE FROM \"{RegisteredClass.DatabaseTableName}\" ";

            if (WhereExpressionParts.Count > 0)
            {
                sql = $"{sql}WHERE ";

                if (allowSqlInjection)
                {
                    foreach (var where in WhereExpressionParts) sql = $"{sql} {where.ToSqlInjectionString()}";
                }
                else
                {
                    foreach (var where in WhereExpressionParts) sql = $"{sql} {where}";
                }                
            }

            return $"{sql} RETURNING *;";
        }

        public async Task<IEnumerable<C>> QueryAsync()
        {
            if (Item is IDBEvent dBEvent) _ = dBEvent.OnDeleteAsync(SqlWriter);

            IEnumerable<C> result = await QueryAsync(QueryType.Delete, ToString()).ConfigureAwait(false);

            if (Item is IDBEvent dBEvent1) _ = dBEvent1.OnDeletedAsync(SqlWriter);

            return result;
        }

        public async Task<List<C>> QueryToListAsync() => (await QueryAsync(QueryType.Delete, ToString()).ConfigureAwait(false)).ToList();

        public async Task<List<T>> QueryToListAsync<T>()
        {
            IEnumerable<C> records = await QueryAsync(QueryType.Delete, ToString()).ConfigureAwait(false);

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
