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
    public sealed class Update<C> : BaseQuery<C> where C : class
    {
        private C? Item { get; set; } = null;

        private List<ExpressionPart<C>> SetExpressionParts { get; set; } = new List<ExpressionPart<C>>();

        private List<ExpressionPart<C>> WhereExpressionParts { get; set; } = new List<ExpressionPart<C>>();

        internal Update(C item, RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, SqlWriterConfiguration queryOptions, ILogger? logger = null)
        {
            SqlWriter = dBAutomator;

            QueryOptions = queryOptions;

            Logger = logger;

            RegisteredClass = registeredClass;

            Item = item;

            Connection = connection;

            Statics.AddParameters(P, Item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement), "s_");

            Statics.AddParameters(P, Item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement && p.IsKey));
            
            if (RegisteredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey && p.IsAutoIncrement))
            {
                Statics.AddParameters(P, Item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey && p.IsAutoIncrement));
            }
            else
            {
                Statics.AddParameters(P, Item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped));
            }
        }

        internal Update(RegisteredClass<C> registeredClass, SqlWriter dBAutomator, IDbConnection connection, SqlWriterConfiguration queryOptions, ILogger? logger = null)
        {
            SqlWriter = dBAutomator;

            QueryOptions = queryOptions;

            Logger = logger;

            RegisteredClass = registeredClass;

            Connection = connection;
        }

        public Update<C> Options(SqlWriterConfiguration queryOptions)
        {
            QueryOptions = queryOptions;

            return this;
        }

        public Update<C> Set(Expression<Func<C, object>> set)
        {
            if (Item != null) throw new SqlWriterException("This method does not support instantiated objects.", new ArgumentException());

            set = set.RemoveClosure()!;

            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(set);

            SetExpressionParts = Statics.GetExpressionParts(binaryExpression, RegisteredClass, null, "s_");

            Statics.AddParameters(P, RegisteredClass, SetExpressionParts, "s_");

            return this;
        }

        public Update<C> Where(Expression<Func<C, object>> where)
        {
            if (Item != null) throw new SqlWriterException("This method does not support instantiated objects.", new ArgumentException());

            where = where.RemoveClosure()!;

            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(where);

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
                return GetSqlByItem(true);
            }
        }

        private string GetSqlByItem(bool allowSqlInjection = false)
        {
            if (Item == null) throw new SqlWriterException("Item must not be null.", new NullReferenceException());

            //string sql = $"UPDATE \"{_registeredClass.DatabaseTableName}\" SET {Statics.ToColumnNameEqualsParameterName(_registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement), "s_", ", ")} WHERE";

            string sql = $"UPDATE \"{RegisteredClass.DatabaseTableName}\" SET ";

            if (allowSqlInjection)
            {
                foreach (var prop in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) sql = $"{sql} {prop.ToSqlInjectionString(Item)}, ";
            }
            else
            {
                foreach (var prop in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) sql = $"{sql} {prop.ToString("s_")}, ";
            }

            if (sql.EndsWith(", ")) sql = sql[..^2];

            sql = $"{sql} WHERE";

            if (WhereExpressionParts?.Count > 0)
            {
                //sql = $"{sql} {Statics.ToColumnNameEqualsParameterName(_registeredClass, _whereExpressionParts)}";

                if (allowSqlInjection)
                {
                    foreach (var where in WhereExpressionParts) sql = $"{sql} {where.ToSqlInjectionString()}";
                }
                else
                {
                    foreach (var where in WhereExpressionParts) sql = $"{sql} {where.ToString()}";
                }

            }
            else
            {
                if (RegisteredClass.RegisteredProperties.Count(p => !p.NotMapped && p.IsKey) == 0) throw new SqlWriterException("The item does not have a key registered nor a where clause.", new ArgumentException());

                //sql = $"{sql} {Statics.ToColumnNameEqualsParameterName(_registeredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey), delimiter: " AND")}";

                if (allowSqlInjection)
                {
                    foreach (var prop in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey)) sql = $"{sql} {prop.ToSqlInjectionString(Item)} AND";
                }
                else
                {
                    foreach (var prop in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey)) sql = $"{sql} {prop.ToString()} AND";
                }
            }

            if (sql.EndsWith("AND")) sql = sql[..^3];

            return $"{sql} RETURNING *;";
        }

        public async Task<C> QueryFirstAsync()
        {
            if (Item is IDBEvent dBEvent) _ = dBEvent.OnUpdateAsync(SqlWriter);

            var result = await QueryFirstAsync(QueryType.Update, ToString());

            Statics.FillDatabaseGeneratedProperties(Item, result, SqlWriter);

            if (Item is DBObject dBObject)
            {
                dBObject.StoreState<C>(SqlWriter);

                dBObject.ObjectState = ObjectState.InDatabase;

                dBObject.QueryType = QueryType.Update;
            }

            if (Item is IDBEvent dBEvent1) _ = dBEvent1.OnUpdatedAsync(SqlWriter);

            return result;
        }

        public async Task<C?> QueryFirstOrDefaultAsync()
        {
            if (Item is IDBEvent dBEvent)
                _ = dBEvent.OnUpdateAsync(SqlWriter);

            var result = await QueryFirstOrDefaultAsync(QueryType.Update, ToString());

            if (result == null)
                return null;

            Statics.FillDatabaseGeneratedProperties(Item, result, SqlWriter);

            if (Item is DBObject dBObject)
            {
                dBObject.StoreState<C>(SqlWriter);

                dBObject.ObjectState = ObjectState.InDatabase;

                dBObject.QueryType = QueryType.Update;
            }

            if (Item is IDBEvent dBEvent1)
                _ = dBEvent1.OnUpdatedAsync(SqlWriter);

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
            string sql = $"UPDATE \"{RegisteredClass.DatabaseTableName}\" SET";

            if (allowSqlInjection)
            {
                foreach (var set in SetExpressionParts) sql = $"{sql} {set.ToSqlInjectionString()}";
            }
            else
            {
                foreach (ExpressionPart<C> set in SetExpressionParts.Where(s => s.ConstantExpression != null && s.MemberExpression != null)) 
                    sql = $"{sql} {set.GetSetString()}, ";
            }

            if (sql.EndsWith(", "))
                sql = sql[0..^2];

            if (WhereExpressionParts.Count > 0)
            {
                sql = $"{sql} WHERE";

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
    }
}
