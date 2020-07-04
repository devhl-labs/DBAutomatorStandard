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
        private readonly C? _item = null;

        private List<IExpression> _setExpressionParts = new List<IExpression>();

        private List<IExpression> _whereExpressionParts = new List<IExpression>();

        private readonly List<IExpression> _orWhereExpressionParts = new List<IExpression>();

        internal Update(C item, RegisteredClass<C> registeredClass, SqlWriter sqlWriter)
        {
            SqlWriter = sqlWriter;

            RegisteredClass = registeredClass;

            _item = item;

            Utils.AddParameters(P, _item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement), "s_");

            Utils.AddParameters(P, _item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement && p.IsKey));
            
            if (RegisteredClass.RegisteredProperties.Any(p => !p.NotMapped && p.IsKey && p.IsAutoIncrement))
            {
                Utils.AddParameters(P, _item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey && p.IsAutoIncrement));
            }
            else
            {
                Utils.AddParameters(P, _item, RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped));
            }
        }

        internal Update(RegisteredClass<C> registeredClass, SqlWriter sqlWriter)
        {
            SqlWriter = sqlWriter;

            RegisteredClass = registeredClass;
        }

        public Update<C> TimeOut(int value)
        {
            CommandTimeOut = value;

            return this;
        }

        public Update<C> Set(Expression<Func<C, object>> set)
        {
            if (_item != null) throw new SqlWriterException("This method does not support instantiated objects.", new ArgumentException());

            set = set.RemoveClosure();

            BinaryExpression? binaryExpression = Utils.GetBinaryExpression(set);

            _setExpressionParts = Utils.GetExpressionParts(this, binaryExpression, RegisteredClass, null, "s_");

            Utils.AddParameters(P, RegisteredClass, _setExpressionParts, "s_");

            return this;
        }

        public Update<C> Where(Expression<Func<C, object>> where)
        {
            if (_item != null) throw new SqlWriterException("This method does not support instantiated objects.", new ArgumentException());

            where = where.RemoveClosure()!;

            BinaryExpression? binaryExpression = Utils.GetBinaryExpression(where);

            _whereExpressionParts = Utils.GetExpressionParts(this, binaryExpression, RegisteredClass);

            Utils.AddParameters(P, RegisteredClass, _whereExpressionParts);

            return this;
        }

        /// <summary>
        /// Use this when building the sql while iterating a collection.  It will result in ...Where() AND ( OrWhere[0] OR OrWhere[1]...);
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public Update<C> OrWhere(Expression<Func<C, object>> where)
        {
            where = where.RemoveClosure();

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
            if (_item == null)
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
            if (_item == null)
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
            if (_item == null) throw new SqlWriterException("Item must not be null.", new NullReferenceException());

            //string sql = $"UPDATE \"{_registeredClass.DatabaseTableName}\" SET {Statics.ToColumnNameEqualsParameterName(_registeredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement), "s_", ", ")} WHERE";

            string sql = $"UPDATE \"{RegisteredClass.DatabaseTableName}\" SET ";

            if (allowSqlInjection)            
                foreach (var prop in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) 
                    sql = $"{sql} {prop.ToSqlInjectionString(_item)}, ";            
            else            
                foreach (var prop in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && !p.IsAutoIncrement)) 
                    sql = $"{sql} {prop.ToString("s_")}, ";            

            if (sql.EndsWith(", ")) sql = sql[..^2];

            sql = $"{sql} WHERE";

            if (_whereExpressionParts?.Count > 0)
            {
                if (allowSqlInjection)                
                    foreach (var where in _whereExpressionParts) 
                        sql = $"{sql} {where.ToSqlInjectionString()}";
                else                
                    foreach (var where in _whereExpressionParts) 
                        sql = $"{sql} {where}"; 
            }
            else
            {
                if (RegisteredClass.RegisteredProperties.Count(p => !p.NotMapped && p.IsKey) == 0) 
                    throw new SqlWriterException("The item does not have a key registered nor a where clause.", new ArgumentException());

                if (allowSqlInjection)                
                    foreach (var prop in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey)) 
                        sql = $"{sql} {prop.ToSqlInjectionString(_item)} AND";                
                else                
                    foreach (var prop in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped && p.IsKey)) 
                        sql = $"{sql} {prop} AND";                
            }

            if (sql.EndsWith("AND")) 
                sql = sql[..^3];

            return $"{sql}";
        }

        public async Task<C> QueryFirstAsync()
        {
            if (_item is IDBEvent dBEvent) 
                _ = dBEvent.OnUpdateAsync(SqlWriter);

            var result = await QueryFirstAsync(QueryType.Update, $"{ToString()} RETURNING *;");

            Utils.FillDatabaseGeneratedProperties(_item, result, SqlWriter);

            if (_item is DBObject dBObject)
            {
                dBObject.StoreState<C>(SqlWriter);

                dBObject.ObjectState = ObjectState.InDatabase;

                dBObject.QueryType = QueryType.Update;
            }

            if (_item is IDBEvent dBEvent1) 
                _ = dBEvent1.OnUpdatedAsync(SqlWriter);

            return result;
        }

        public async Task<C?> QueryFirstOrDefaultAsync()
        {
            if (_item is IDBEvent dBEvent)
                _ = dBEvent.OnUpdateAsync(SqlWriter);

            var result = await QueryFirstOrDefaultAsync(QueryType.Update, $"{ToString()} RETURNING *;");

            if (result == null)
                return null;

            Utils.FillDatabaseGeneratedProperties(_item, result, SqlWriter);

            if (_item is DBObject dBObject)
            {
                dBObject.StoreState<C>(SqlWriter);

                dBObject.ObjectState = ObjectState.InDatabase;

                dBObject.QueryType = QueryType.Update;
            }

            if (_item is IDBEvent dBEvent1)
                _ = dBEvent1.OnUpdatedAsync(SqlWriter);

            return result;
        }

        public async Task<List<C>> QueryToListAsync() => (await QueryAsync(QueryType.Update, $"{ToString()} RETURNING *;").ConfigureAwait(false)).ToList();

        public async Task<List<T>> QueryToListAsync<T>()
        {
            IEnumerable<C> records = await QueryAsync(QueryType.Update, $"{ToString()} RETURNING *;").ConfigureAwait(false);

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
                foreach (var set in _setExpressionParts) 
                    sql = $"{sql} {set.ToSqlInjectionString()}";            
            else            
                foreach (BodyExpression<C> set in _setExpressionParts.Where(p => p is BodyExpression<C>)) //.Where(s => s.ConstantExpression != null && s.MemberExpression != null)) 
                    sql = $"{sql} {set.GetSetString()}, ";           

            if (sql.EndsWith(", "))
                sql = sql[0..^2];

            if (_whereExpressionParts.Count > 0 || _orWhereExpressionParts.Count > 0)
                sql = $"{sql} WHERE";

            if (_whereExpressionParts.Count > 0)
            {
                if (allowSqlInjection)                
                    foreach (var where in _whereExpressionParts) 
                        sql = $"{sql} {where.ToSqlInjectionString()}";                
                else                
                    foreach (var where in _whereExpressionParts) 
                        sql = $"{sql} {where}";                
            }

            if (_orWhereExpressionParts.Count > 0)
            {
                if (_whereExpressionParts.Count > 0)
                    sql = $"{sql} AND (";

                if (allowSqlInjection)
                    foreach (var where in _orWhereExpressionParts)
                        sql = $"{sql} {where.ToSqlInjectionString()}";
                else
                    foreach (var where in _orWhereExpressionParts)
                        sql = $"{sql} {where}";

                sql = sql[..^2];

                if (_whereExpressionParts.Count > 0)
                    sql = $"{sql})";
            }

            return $"{sql}";
        }

        public async Task ExecuteAsync()
        {
            if (_item is IDBEvent dBEvent)
                _ = dBEvent.OnUpdateAsync(SqlWriter);

            await ExecuteAsync($"{ToString()};");

            if (_item is DBObject dBObject)
            {
                dBObject.StoreState<C>(SqlWriter);

                dBObject.ObjectState = ObjectState.InDatabase;

                dBObject.QueryType = QueryType.Update;
            }

            if (_item is IDBEvent dBEvent1)
                _ = dBEvent1.OnUpdatedAsync(SqlWriter);
        }
    }
}
