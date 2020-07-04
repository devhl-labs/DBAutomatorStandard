using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Dapper.SqlWriter.Interfaces;
using Dapper.SqlWriter.Models;
using System.Data;

namespace Dapper.SqlWriter 
{
    public sealed class Delete<C> : BaseQuery<C> where C : class
    {
        private List<IExpression> WhereExpressionParts { get; set; } = new List<IExpression>();

        private readonly List<IExpression> _orWhereExpressionParts = new List<IExpression>();

        private C? Item { get; set; }

        internal Delete(RegisteredClass<C> registeredClass, SqlWriter sqlWriter)
        {
            SqlWriter = sqlWriter;

            RegisteredClass = registeredClass;

            CommandTimeOut = sqlWriter.Config.CommandTimeOut;
        }

        public Delete<C> TimeOut(int value)
        {
            CommandTimeOut = value;

            return this;
        }

        internal Delete(C item, RegisteredClass<C> registeredClass, SqlWriter dBAutomator)
        {
            SqlWriter = dBAutomator;

            RegisteredClass = registeredClass;

            Item = item ?? throw new SqlWriterException("Item must not be null.", new ArgumentException());

            if (RegisteredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey))
            {
                Utils.AddParameters(P, Item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey));
            }
            else
            {
                Utils.AddParameters(P, Item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped));
            }
        }

        public Delete<C> Where(Expression<Func<C, object>> where)
        {
            where = where.RemoveClosure();

            BinaryExpression binaryExpression = Utils.GetBinaryExpression(where);

            WhereExpressionParts = Utils.GetExpressionParts(this, binaryExpression, RegisteredClass, null);

            Utils.AddParameters(P, RegisteredClass, WhereExpressionParts);

            return this;
        }

        /// <summary>
        /// Use this when building the sql while iterating a collection.  It will result in ...Where() AND ( OrWhere[0] OR OrWhere[1]...);
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public Delete<C> OrWhere(Expression<Func<C, object>> where)
        {
            where = where.RemoveClosure()!;

            BinaryExpression? binaryExpression = Utils.GetBinaryExpression(where);

            _orWhereExpressionParts.Add(new ParenthesisExpression(Parens.Left));

            _orWhereExpressionParts.AddRange(Utils.GetExpressionParts(this, binaryExpression, RegisteredClass, null, "orw_"));

            Utils.AddParameters(P, RegisteredClass, _orWhereExpressionParts, "orw_");

            _orWhereExpressionParts.Add(new ParenthesisExpression(Parens.Right));

            _orWhereExpressionParts.Add(new NodeExpression(ExpressionType.OrElse));

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

            return sql; // $"{sql} RETURNING *;";
        }

        private string GetSqlByExpression(bool allowSqlInjection = false)
        {
            string sql = $"DELETE FROM \"{RegisteredClass.DatabaseTableName}\" ";

            if (WhereExpressionParts.Count > 0 || _orWhereExpressionParts.Count > 0)
                sql = $"{sql} WHERE ";

            if (WhereExpressionParts.Count > 0)
            {
                if (allowSqlInjection)
                    foreach (var where in WhereExpressionParts) sql = $"{sql} {where.ToSqlInjectionString()}";
                else
                    foreach (var where in WhereExpressionParts) sql = $"{sql} {where}";
            }

            if (_orWhereExpressionParts.Count > 0)
            {
                if (WhereExpressionParts.Count > 0)
                    sql = $"{sql} AND (";

                if (allowSqlInjection)
                    foreach (var where in _orWhereExpressionParts)
                        sql = $"{sql} {where.ToSqlInjectionString()}";
                else
                    foreach (var where in _orWhereExpressionParts)
                        sql = $"{sql} {where}";

                sql = sql[..^2];

                if (WhereExpressionParts.Count > 0)
                    sql = $"{sql})";
            }

            return sql;
        }

        public async Task ExecuteAsync()
        {
            if (Item is IDBEvent dBEvent)
                _ = dBEvent.OnDeleteAsync(SqlWriter);

            await ExecuteAsync(ToString()).ConfigureAwait(false);

            if (Item is IDBEvent dBEvent1)
                _ = dBEvent1.OnDeletedAsync(SqlWriter);
        }

        public async Task<IEnumerable<C>> QueryAsync()
        {
            if (Item is IDBEvent dBEvent) _ = dBEvent.OnDeleteAsync(SqlWriter);

            IEnumerable<C> result = await QueryAsync(QueryType.Delete, $"{ToString()} RETURNING *;").ConfigureAwait(false);

            if (Item is IDBEvent dBEvent1) _ = dBEvent1.OnDeletedAsync(SqlWriter);

            return result;
        }

        public async Task<List<C>> QueryToListAsync() => (await QueryAsync(QueryType.Delete, $"{ToString()} RETURNING *;").ConfigureAwait(false)).ToList();

        public async Task<List<T>> QueryToListAsync<T>()
        {
            IEnumerable<C> records = await QueryAsync(QueryType.Delete, $"{ToString()} RETURNING *;").ConfigureAwait(false);

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























//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;

//using Dapper.SqlWriter.Interfaces;
//using Dapper.SqlWriter.Models;
//using System.Data;

//namespace Dapper.SqlWriter
//{
//    public sealed class Delete<C> : BaseQuery<C> where C : class
//    {
//        private List<ExpressionPart<C>> WhereExpressionParts { get; set; } = new List<ExpressionPart<C>>();

//        private readonly List<ExpressionPart<C>> _orWhereExpressionParts = new List<ExpressionPart<C>>();

//        private C? Item { get; set; }

//        internal Delete(RegisteredClass<C> registeredClass, SqlWriter sqlWriter)
//        {
//            SqlWriter = sqlWriter;

//            RegisteredClass = registeredClass;

//            CommandTimeOut = sqlWriter.Config.CommandTimeOut;
//        }

//        public Delete<C> TimeOut(int value)
//        {
//            CommandTimeOut = value;

//            return this;
//        }

//        internal Delete(C item, RegisteredClass<C> registeredClass, SqlWriter dBAutomator)
//        {
//            SqlWriter = dBAutomator;

//            RegisteredClass = registeredClass;

//            Item = item ?? throw new SqlWriterException("Item must not be null.", new ArgumentException());

//            if (RegisteredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey))
//            {
//                Statics.AddParameters(P, Item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey));
//            }
//            else
//            {
//                Statics.AddParameters(P, Item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped));
//            }
//        }

//        public Delete<C> Where(Expression<Func<C, object>> where)
//        {
//            where = where.RemoveClosure();

//            BinaryExpression binaryExpression = Statics.GetBinaryExpression(where);

//            WhereExpressionParts = Statics.GetExpressionParts(binaryExpression, RegisteredClass);

//            Statics.AddParameters(P, RegisteredClass, WhereExpressionParts);

//            return this;
//        }

//        /// <summary>
//        /// Use this when building the sql while iterating a collection.  It will result in ...Where() AND ( OrWhere[0] OR OrWhere[1]...);
//        /// </summary>
//        /// <param name="where"></param>
//        /// <returns></returns>
//        public Delete<C> OrWhere(Expression<Func<C, object>> where)
//        {
//            where = where.RemoveClosure()!;

//            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(where);

//            _orWhereExpressionParts.Add(new ExpressionPart<C> { Parens = Parens.Left });

//            _orWhereExpressionParts.AddRange(Statics.GetExpressionParts(binaryExpression, RegisteredClass, null, "orw_"));

//            Statics.AddParameters(P, RegisteredClass, _orWhereExpressionParts, "orw_");

//            _orWhereExpressionParts.Add(new ExpressionPart<C> { Parens = Parens.Right });

//            _orWhereExpressionParts.Add(new ExpressionPart<C> { NodeType = ExpressionType.OrElse });

//            return this;
//        }

//        public override string ToString()
//        {
//            if (Item == null)
//            {
//                return GetSqlByExpression();
//            }
//            else
//            {
//                return GetSqlByItem();
//            }
//        }

//        public string ToSqlInjectionString()
//        {
//            if (Item == null)
//            {
//                return GetSqlByExpression(true);
//            }
//            else
//            {
//                return GetSqlByItem();
//            }
//        }

//        private string GetSqlByItem(bool allowSqlInjection = false)
//        {
//            if (Item == null)
//                throw new SqlWriterException("Item must not be null.", new ArgumentException());

//            string sql = $"DELETE FROM \"{RegisteredClass.DatabaseTableName}\" WHERE";

//            if (RegisteredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey))
//            {
//                foreach (RegisteredProperty<C> prop in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey))
//                {
//                    if (allowSqlInjection && Item != null)
//                    {
//                        sql = $"{sql} {prop.ToSqlInjectionString(Item)} AND";
//                    }
//                    else
//                    {
//                        sql = $"{sql} {prop} AND";
//                    }
//                }
//            }
//            else
//            {
//                foreach (var prop in RegisteredClass.RegisteredProperties.Where(p => p.NotMapped))
//                {
//                    if (allowSqlInjection)
//                    {
//                        sql = $"{sql} {prop.ToSqlInjectionString(Item)} AND";
//                    }
//                    else
//                    {
//                        sql = $"{sql} {prop} AND";
//                    }
//                }
//            }

//            if (sql.EndsWith(" AND"))
//                sql = sql[..^4];

//            return sql; // $"{sql} RETURNING *;";
//        }

//        private string GetSqlByExpression(bool allowSqlInjection = false)
//        {
//            string sql = $"DELETE FROM \"{RegisteredClass.DatabaseTableName}\" ";

//            if (WhereExpressionParts.Count > 0 || _orWhereExpressionParts.Count > 0)
//                sql = $"{sql} WHERE ";

//            if (WhereExpressionParts.Count > 0)
//            {
//                if (allowSqlInjection)
//                    foreach (var where in WhereExpressionParts)
//                        sql = $"{sql} {where.ToSqlInjectionString()}";
//                else
//                    foreach (var where in WhereExpressionParts)
//                        sql = $"{sql} {where}";
//            }

//            if (_orWhereExpressionParts.Count > 0)
//            {
//                if (WhereExpressionParts.Count > 0)
//                    sql = $"{sql} AND (";

//                if (allowSqlInjection)
//                    foreach (var where in _orWhereExpressionParts)
//                        sql = $"{sql} {where.ToSqlInjectionString()}";
//                else
//                    foreach (var where in _orWhereExpressionParts)
//                        sql = $"{sql} {where}";

//                sql = sql[..^2];

//                if (WhereExpressionParts.Count > 0)
//                    sql = $"{sql})";
//            }

//            return sql; // $"{sql} RETURNING *;";
//        }

//        public async Task ExecuteAsync()
//        {
//            if (Item is IDBEvent dBEvent)
//                _ = dBEvent.OnDeleteAsync(SqlWriter);

//            await ExecuteAsync(ToString()).ConfigureAwait(false);

//            if (Item is IDBEvent dBEvent1)
//                _ = dBEvent1.OnDeletedAsync(SqlWriter);
//        }

//        public async Task<IEnumerable<C>> QueryAsync()
//        {
//            if (Item is IDBEvent dBEvent)
//                _ = dBEvent.OnDeleteAsync(SqlWriter);

//            IEnumerable<C> result = await QueryAsync(QueryType.Delete, $"{ToString()} RETURNING *;").ConfigureAwait(false);

//            if (Item is IDBEvent dBEvent1)
//                _ = dBEvent1.OnDeletedAsync(SqlWriter);

//            return result;
//        }

//        public async Task<List<C>> QueryToListAsync() => (await QueryAsync(QueryType.Delete, $"{ToString()} RETURNING *;").ConfigureAwait(false)).ToList();

//        public async Task<List<T>> QueryToListAsync<T>()
//        {
//            IEnumerable<C> records = await QueryAsync(QueryType.Delete, $"{ToString()} RETURNING *;").ConfigureAwait(false);

//            List<T> results = new List<T>();

//            foreach (C record in records)
//            {
//                object recordObject = record;

//                results.Add((T)recordObject);
//            }

//            return results;
//        }
//    }
//}
