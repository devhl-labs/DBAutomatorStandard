using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper.SqlWriter.Models;
using System.Data;

namespace Dapper.SqlWriter
{
    public sealed class Get<C> : BaseQuery<C> where C : class
    {
        private IEnumerable<C>? _results = null;

        private readonly List<BodyExpression<C>> _selectExpressionParts = new List<BodyExpression<C>>();

        private int? _topValue = null;

        private int? _limitValue = null;

        private double? _topPercentValue = null;

        private Comparison _comparison;

        private int? _rowNumValue = null;

        private List<BodyExpression<C>> _whereExpressionParts = new List<BodyExpression<C>>();

        private readonly List<BodyExpression<C>> _orWhereExpressionParts = new List<BodyExpression<C>>();

        //private readonly string _tableNameValue = string.Empty;

        private bool _isDistinct;

        private List<OrderByExpression<C>> _orderByExpressionParts = new List<OrderByExpression<C>>();

        internal Get(RegisteredClass<C> registeredClass, SqlWriter sqlWriter)
        {
            SqlWriter = sqlWriter;

            RegisteredClass = registeredClass;

            Select<C>();
        }

        private string _columns = string.Empty;

        private string _splitOn = string.Empty;

        public Get<C> Select<T>() where T : class
        {
            RegisteredClass<T> registeredClass = (RegisteredClass<T>) SqlWriter.RegisteredClasses.First(r => r is RegisteredClass<T>);

            if (_columns != string.Empty)
                _columns += ", ";

            foreach(RegisteredProperty<T> prop in registeredClass.RegisteredProperties.Where(p => p.NotMapped == false))
                _columns += $"\"{registeredClass.DatabaseTableName}\".\"{prop.ColumnName}\", ";

            _columns = _columns[..^2];

            if (_splitOn == string.Empty)
                _splitOn += registeredClass.RegisteredProperties.First().ColumnName;
            else
                _splitOn += ", " + registeredClass.RegisteredProperties.First().ColumnName;

            return this;
        }

        //public Get<C> TableName(string tableName)
        //{
        //    _tableNameValue = tableName;

        //    return this;
        //}

        public Get<C> TimeOut(int value)
        {
            CommandTimeOut = value;

            return this;
        }

        //public Get<C> Column(Expression<Func<C, object>> column)
        //{
        //    column = column.RemoveClosure();

        //    ExpressionPart<C> part = new ExpressionPart<C>
        //    {
        //        MemberExpression = Statics.GetMemberExpression(column)
        //    };

        //    _selectExpressionParts.Add(part);

        //    return this;
        //}

        public Get<C> Distinct()
        {
            _isDistinct = true;

            return this;
        }

        private string _where = string.Empty;

        public Get<C> Where(Expression<Func<C, object>> where) => Where<C>(where);

        public Get<C> Where<T>(Expression<Func<T, object>> where) where T : class
        {
            where = where.RemoveClosure();

            BinaryExpression? binaryExpression = Utils.GetBinaryExpression(where);

            RegisteredClass<T> registeredClass = (RegisteredClass<T>) SqlWriter.RegisteredClasses.First(r => r is RegisteredClass<T>);

            var whereParts = Utils.GetExpressionParts(this, binaryExpression, registeredClass);

            Utils.AddParameters(P, registeredClass, whereParts);

            if (_where != string.Empty)
                _where += "AND ";

            foreach (IExpression wherePart in whereParts)
                _where += wherePart + " ";

            //_where = _where[..^5];
            //_where = _where[..^1];
            return this;
        }

        private string _orWhere = string.Empty;

        public Get<C> OrWhere(Expression<Func<C, object>> where) => OrWhere<C>(where);

        /// <summary>
        /// Use this when building the sql while iterating a collection.  It will result in ...Where() AND ( OrWhere[0] OR OrWhere[1]...);
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public Get<C> OrWhere<T>(Expression<Func<T, object>> where) where T : class
        {
            where = where.RemoveClosure();

            RegisteredClass<T> registeredClass = (RegisteredClass<T>) SqlWriter.RegisteredClasses.First(r => r is RegisteredClass<T>);

            BinaryExpression? binaryExpression = Utils.GetBinaryExpression(where);

            List<IExpression> orWhereParts = new List<IExpression>
            {
                new ParenthesisExpression(Parens.Left)
            };

            orWhereParts.AddRange(Utils.GetExpressionParts(this, binaryExpression, registeredClass, null, "orw_"));

            orWhereParts.Add(new ParenthesisExpression(Parens.Right));

            //orWhereParts.Add(new NodeTypePart(ExpressionType.OrElse));

            Utils.AddParameters(P, registeredClass, orWhereParts, "orw_");

            if (_orWhere == string.Empty)
            {
                foreach (var w in orWhereParts)
                    _orWhere += w.ToString();

                _orWhere += "OR ";
            }
            else
            {
                foreach (var w in orWhereParts)
                    _orWhere += w.ToString() + " ";

                _orWhere += "OR ";
            }
            // OR );
            return this;
        }

        public Get<C> OrderBy(Expression<Func<C, object>> orderBy) => OrderBy<C>(orderBy);

        public Get<C> OrderBy<T>(Expression<Func<T, object>> orderBy) where T : class => OrderBy(orderBy, true);

        public Get<C> OrderByDesc(Expression<Func<C, object>> orderBy) => OrderByDesc<C>(orderBy);

        public Get<C> OrderByDesc<T>(Expression<Func<T, object>> orderBy) where T : class => OrderBy(orderBy, false);

        //private string _orderBy = string.Empty;

        private Get<C> OrderBy<T>(Expression<Func<T, object>> orderBy, bool ascending) where T : class
        {
            orderBy = orderBy.RemoveClosure();

            //ExpressionPart<C> part = new ExpressionPart<C>
            //{
            MemberExpression memberExpression = Utils.GetMemberExpression(orderBy);
            //};

            //if (ascending) 
            //    part.NodeType = ExpressionType.GreaterThan;

            //if (!ascending) 
            //    part.NodeType = ExpressionType.LessThan;

            RegisteredProperty<C> registeredProperty = RegisteredClass.RegisteredProperties.First(p => p.Property.Name == memberExpression.Member.Name);

            _orderByExpressionParts.Add(new OrderByExpression<C>(registeredProperty, memberExpression, (ascending) ? ExpressionType.GreaterThan : ExpressionType.LessThan));

            //RegisteredClass<T> registeredClass = (RegisteredClass<T>) SqlWriter.RegisteredClasses.First(r => r is RegisteredClass<T>);

            //foreach (var expressionPart in _orderByExpressionParts)
            //{
            //    RegisteredProperty<T> registeredProperty = registeredClass.RegisteredProperties.First(p => p.PropertyName == expressionPart.MemberExpression?.Member.Name);

            //    string orderByPart = $"\"{registeredProperty.ColumnName}\" ASC, ";

            //    if (expressionPart.NodeType == ExpressionType.LessThan)
            //        orderByPart = $"\"{registeredProperty.ColumnName}\" DESC, ";

            //    _orderBy += orderByPart;
            //}

            //_orderBy = _orderBy[..^2];

            return this;
        }

        /// <summary>
        /// MySQL, Postgres, and others
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Get<C> Limit(int limit)
        {
            _limitValue = limit;

            return this;
        }

        /// <summary>
        /// SQL Server specific
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        public Get<C> Top(int top)
        {
            _topValue = top;

            return this;
        }

        /// <summary>
        /// SQL Server specific
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        public Get<C> TopPercent(int top)
        {
            _topPercentValue = top;

            return this;
        }

        /// <summary>
        /// Oracle specific
        /// </summary>
        /// <param name="comparison"></param>
        /// <param name="rowNum"></param>
        /// <returns></returns>
        public Get<C> RowNum(Comparison comparison, int rowNum)
        {
            _comparison = comparison;

            _rowNumValue = rowNum;

            return this;
        }

        //public string ToSqlInjectionString() => GetString(true);

        public override string ToString() => GetString();

        //private string _joinColumns = string.Empty;

        private string _joinOn = string.Empty;

        public Get<C> LeftJoin<T>(Expression<Func<C, T, object>> on) where T : class 
            => Join("LEFT JOIN", on);

        public Get<C> RightJoin<T>(Expression<Func<C, T, object>> on) where T : class
            => Join("RIGHT JOIN", on);

        public Get<C> Join<T>(Expression<Func<C, T, object>> on) where T : class
            => Join("JOIN", on);

        public Get<C> LeftJoin<T1, T2>(Expression<Func<T1, T2, object>> on) where T1 : class where T2 : class
            => Join("LEFT JOIN", on);

        public Get<C> RightJoin<T1, T2>(Expression<Func<T1, T2, object>> on) where T1 : class where T2 : class
            => Join("RIGHT JOIN", on);

        public Get<C> Join<T1, T2>(Expression<Func<T1, T2, object>> on) where T1 : class where T2 : class
            => Join("JOIN", on);

        private Get<C> Join<T1, T2>(string join, Expression<Func<T1, T2, object>> on) where T1 : class where T2 : class
        {
            RegisteredClass<T1> registeredClass1 = (RegisteredClass<T1>) SqlWriter.RegisteredClasses.First(r => r is RegisteredClass<T1>);

            RegisteredClass<T2> registeredClass2 = (RegisteredClass<T2>)SqlWriter.RegisteredClasses.First(r => r is RegisteredClass<T2>);

            //foreach (var prop in registeredClass.RegisteredProperties.Where(p => p.NotMapped == false))
            //    _joinColumns += $"\"{registeredClass.DatabaseTableName}\".\"{prop.ColumnName}\", ";

            var unaryBody = (UnaryExpression)on.Body;

            var binaryOperand = ((BinaryExpression)unaryBody.Operand);

            var leftMember = ((MemberExpression)binaryOperand.Left).Member.Name;

            var rightMember = ((MemberExpression)binaryOperand.Right).Member.Name;

            _joinOn += $"{join} \"{registeredClass2.DatabaseTableName}\" ON \"{registeredClass1.DatabaseTableName}\".\"{leftMember}\" = \"{registeredClass2.DatabaseTableName}\".\"{rightMember}\"";

            return this;
        }

        private string GetString(bool allowSqlInjection = false)
        {
            string sql = $"SELECT ";

            if (_isDistinct)            
                sql = $"{sql}DISTINCT ";            

            if (_topValue != null)            
                sql = $"{sql}TOP({_topValue}) ";            
            else if (_topPercentValue != null)            
                sql = $"{sql}TOP({_topPercentValue}) PERCENT ";

            sql = $"{sql}{_columns} ";

            //if (_selectExpressionParts.Count == 0)            
            //    foreach (var property in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped))                
            //        sql = $"{sql}\"{RegisteredClass.DatabaseTableName}\".\"{property.ColumnName}\", ";              
            //else
            //{
            //    foreach (var expression in _selectExpressionParts.Where(e => e.MemberExpression != null))
            //    {
            //        RegisteredProperty<C> registeredProperty = Statics.GetRegisteredProperty(RegisteredClass, expression.MemberExpression!);

            //        sql = $"{sql}\"{registeredProperty.ColumnName}\", ";
            //    }
            //}

            //sql += _joinColumns;

            //sql = sql[0..^2];

            sql = $"{sql}FROM \"{RegisteredClass.DatabaseTableName}\" ";

            sql = $"{sql}{_joinOn} ";

            if (_orWhere != string.Empty)
                _orWhere = _orWhere[..^3];

            if (_where != string.Empty || _orWhere != string.Empty || _rowNumValue != null)            
                sql = $"{sql}WHERE ";  
            
            if (_rowNumValue != null)
                sql = $"{sql}ROWNUM {_comparison.GetOperator()} {_rowNumValue} ";

            if (_where != string.Empty && _orWhere != string.Empty)
                sql = $"{sql}({_where}) OR ({_orWhere}) ";
            else if (_where != string.Empty)
                sql = $"{sql}{_where} ";
            else if (_orWhere != string.Empty)
                sql = $"{sql}{_orWhere} ";



            //if (_whereExpressionParts.Count > 0)
            //{         
            //    if (allowSqlInjection)                
            //        foreach (var where in _whereExpressionParts) sql = $"{sql} {where.ToSqlInjectionString()}";                
            //    else                
            //        foreach (var where in _whereExpressionParts) sql = $"{sql} {where}";                
            //}

            //if (_orWhereExpressionParts.Count > 0)
            //{
            //    if (_whereExpressionParts.Count > 0)
            //        sql = $"{sql} AND (";

            //    if (allowSqlInjection)                
            //        foreach (var where in _orWhereExpressionParts)
            //            sql = $"{sql} {where.ToSqlInjectionString()}";                
            //    else                
            //        foreach (var where in _orWhereExpressionParts)
            //            sql = $"{sql} {where}";                

            //    sql = sql[..^2];

            //    if (_whereExpressionParts.Count > 0)
            //        sql = $"{sql})";
            //}

            if (_orderByExpressionParts.Count != 0)
            {
                sql = $"{sql}ORDER BY ";

                foreach (var orderBy in _orderByExpressionParts)
                    sql = $"{sql}{orderBy}, ";

                sql = $"{sql[..^2]} ";
            }




            //if (_orderBy != string.Empty)
            //    sql = $"{sql}ORDER BY {_orderBy} ";

            //if (_orderByExpressionParts.Count > 0)
            //{
            //    sql = $"{sql} ORDER BY";

            //    foreach (var expressionPart in _orderByExpressionParts)
            //    {
            //        RegisteredProperty<C> registeredProperty = RegisteredClass.RegisteredProperties.First(p => p.PropertyName == expressionPart.MemberExpression?.Member.Name);

            //        if (expressionPart.NodeType == ExpressionType.GreaterThan) sql = $"{sql} \"{registeredProperty.ColumnName}\" ASC";

            //        if (expressionPart.NodeType == ExpressionType.LessThan) sql = $"{sql} \"{registeredProperty.ColumnName}\" DESC";
            //    }
            //}

            if (_limitValue != null) 
                sql = $"{sql}LIMIT {_limitValue} ";

            return $"{sql[..^1]};";
        }

        public async Task<IEnumerable<C>> QueryAsync() => await QueryAsync(QueryType.Select, ToString(), _splitOn).ConfigureAwait(false);

        public async Task<C> QueryFirstAsync() => await QueryFirstAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<C?> QueryFirstOrDefaultAsync() => await QueryFirstOrDefaultAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<C> QuerySingleAsync() => await QuerySingleAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<C?> QuerySingleOrDefaultAsync() => await QuerySingleOrDefaultAsync(QueryType.Select, ToString()).ConfigureAwait(false);

        public async Task<List<C>> QueryToListAsync() => (await QueryAsync(QueryType.Select, ToString()).ConfigureAwait(false)).ToList();

        public async Task<List<T>> QueryToListAsync<T>() where T : class
        {
            _results = await QueryAsync(QueryType.Select, ToString()).ConfigureAwait(false);

            List<T> results = new List<T>();

            foreach (var result in _results)
            {
                object recordObject = result;

                T item = (T) recordObject;

                results.Add(item);
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
//using Dapper.SqlWriter.Models;
//using System.Data;

//namespace Dapper.SqlWriter
//{
//    public sealed class Get<C> : BaseQuery<C> where C : class
//    {
//        private IEnumerable<C>? _results = null;

//        private readonly List<ExpressionPart<C>> _selectExpressionParts = new List<ExpressionPart<C>>();

//        private int? _topValue = null;

//        private int? _limitValue = null;

//        private double? _topPercentValue = null;

//        private Comparison _comparison;

//        private int? _rowNumValue = null;

//        private List<ExpressionPart<C>> _whereExpressionParts = new List<ExpressionPart<C>>();

//        private readonly List<ExpressionPart<C>> _orWhereExpressionParts = new List<ExpressionPart<C>>();

//        //private readonly string _tableNameValue = string.Empty;

//        private bool _isDistinct;

//        private List<ExpressionPart<C>> _orderByExpressionParts = new List<ExpressionPart<C>>();

//        internal Get(RegisteredClass<C> registeredClass, SqlWriter sqlWriter)
//        {
//            SqlWriter = sqlWriter;

//            RegisteredClass = registeredClass;

//            Select<C>();
//        }

//        private readonly string _columns = string.Empty;

//        public Get<C> Select<T>() where T : class
//        {
//            RegisteredClass<T> registeredClass = (RegisteredClass<T>)SqlWriter.RegisteredClasses.First(r => r is RegisteredClass<T>);

//            foreach (RegisteredProperty<T> prop in registeredClass.RegisteredProperties.Where(p => p.NotMapped == false))


//                return this;
//        }

//        //public Get<C> TableName(string tableName)
//        //{
//        //    _tableNameValue = tableName;

//        //    return this;
//        //}

//        public Get<C> TimeOut(int value)
//        {
//            CommandTimeOut = value;

//            return this;
//        }

//        public Get<C> Column(Expression<Func<C, object>> column)
//        {
//            column = column.RemoveClosure();

//            ExpressionPart<C> part = new ExpressionPart<C>
//            {
//                MemberExpression = Statics.GetMemberExpression(column)
//            };

//            _selectExpressionParts.Add(part);

//            return this;
//        }

//        public Get<C> Distinct()
//        {
//            _isDistinct = true;

//            return this;
//        }

//        public Get<C> Where(Expression<Func<C, object>> where)
//        {
//            where = where.RemoveClosure();

//            BinaryExpression? binaryExpression = Statics.GetBinaryExpression(where);

//            _whereExpressionParts = Statics.GetExpressionParts(binaryExpression, RegisteredClass);

//            Statics.AddParameters(P, RegisteredClass, _whereExpressionParts);

//            return this;
//        }

//        /// <summary>
//        /// Use this when building the sql while iterating a collection.  It will result in ...Where() AND ( OrWhere[0] OR OrWhere[1]...);
//        /// </summary>
//        /// <param name="where"></param>
//        /// <returns></returns>
//        public Get<C> OrWhere(Expression<Func<C, object>> where)
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

//        //public Select<C> AndWhere(Expression<Func<C, object>> where)
//        //{
//        //    where = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

//        //    BinaryExpression? binaryExpression = Statics.GetBinaryExpression(where);

//        //    _andWhereExpressionParts.AddRange(Statics.GetExpressionParts(binaryExpression, _registeredClass));

//        //    Statics.AddParameters(_p, _registeredClass, _orWhereExpressionParts);

//        //    return this;
//        //}

//        public Get<C> OrderBy(Expression<Func<C, object>> orderBy) => OrderBy(orderBy, true);

//        public Get<C> OrderByDesc(Expression<Func<C, object>> orderBy) => OrderBy(orderBy, false);

//        private Get<C> OrderBy(Expression<Func<C, object>> orderBy, bool ascending)
//        {
//            orderBy = orderBy.RemoveClosure();

//            _orderByExpressionParts ??= new List<ExpressionPart<C>>();

//            ExpressionPart<C> part = new ExpressionPart<C>
//            {
//                MemberExpression = Statics.GetMemberExpression(orderBy)
//            };

//            if (ascending)
//                part.NodeType = ExpressionType.GreaterThan;

//            if (!ascending)
//                part.NodeType = ExpressionType.LessThan;

//            _orderByExpressionParts.Add(part);

//            return this;
//        }

//        /// <summary>
//        /// MySQL, Postgres, and others
//        /// </summary>
//        /// <param name="limit"></param>
//        /// <returns></returns>
//        public Get<C> Limit(int limit)
//        {
//            _limitValue = limit;

//            return this;
//        }

//        /// <summary>
//        /// SQL Server specific
//        /// </summary>
//        /// <param name="top"></param>
//        /// <returns></returns>
//        public Get<C> Top(int top)
//        {
//            _topValue = top;

//            return this;
//        }

//        /// <summary>
//        /// SQL Server specific
//        /// </summary>
//        /// <param name="top"></param>
//        /// <returns></returns>
//        public Get<C> TopPercent(int top)
//        {
//            _topPercentValue = top;

//            return this;
//        }

//        /// <summary>
//        /// Oracle specific
//        /// </summary>
//        /// <param name="comparison"></param>
//        /// <param name="rowNum"></param>
//        /// <returns></returns>
//        public Get<C> RowNum(Comparison comparison, int rowNum)
//        {
//            _comparison = comparison;

//            _rowNumValue = rowNum;

//            return this;
//        }

//        public string ToSqlInjectionString() => GetString(true);

//        public override string ToString() => GetString();

//        private string _joinColumns = string.Empty;

//        private string _joinOn = string.Empty;

//        public Get<C> LeftJoin<TJoin>(Expression<Func<C, TJoin, object>> on) where TJoin : class
//        {
//            RegisteredClass<TJoin> registeredClass = (RegisteredClass<TJoin>)SqlWriter.RegisteredClasses.First(r => r is RegisteredClass<TJoin>);

//            foreach (var prop in registeredClass.RegisteredProperties.Where(p1 => p1.NotMapped == false && RegisteredClass.RegisteredProperties.Count(p2 => p2.ColumnName == p1.ColumnName) == 0))
//                //foreach (var prop in registeredClass.RegisteredProperties.Where(p1 => p1.NotMapped == false))
//                _joinColumns += $"\"{registeredClass.DatabaseTableName}\".\"{prop.ColumnName}\", ";

//            var unaryBody = (UnaryExpression)on.Body;

//            var binaryOperand = ((BinaryExpression)unaryBody.Operand);

//            var leftMember = ((MemberExpression)binaryOperand.Left).Member.Name;

//            var rightMember = ((MemberExpression)binaryOperand.Right).Member.Name;

//            _joinOn += $"LEFT JOIN \"{registeredClass.DatabaseTableName}\" ON \"{RegisteredClass.DatabaseTableName}\".\"{leftMember}\" = \"{registeredClass.DatabaseTableName}\".\"{rightMember}\"";

//            return this;
//        }

//        private string GetString(bool allowSqlInjection = false)
//        {
//            string sql = $"SELECT ";

//            if (_isDistinct)
//                sql = $"{sql}DISTINCT ";


//            if (_topValue != null)
//                sql = $"{sql}TOP({_topValue}) ";
//            else if (_topPercentValue != null)
//                sql = $"{sql}TOP({_topPercentValue}) PERCENT ";

//            if (_selectExpressionParts.Count == 0)
//                foreach (var property in RegisteredClass.RegisteredProperties.Where(p => !p.NotMapped))
//                    sql = $"{sql}\"{RegisteredClass.DatabaseTableName}\".\"{property.ColumnName}\", ";
//            else
//            {
//                foreach (var expression in _selectExpressionParts.Where(e => e.MemberExpression != null))
//                {
//                    RegisteredProperty<C> registeredProperty = Statics.GetRegisteredProperty(RegisteredClass, expression.MemberExpression!);

//                    sql = $"{sql}\"{registeredProperty.ColumnName}\", ";
//                }
//            }

//            sql += _joinColumns;

//            sql = sql[0..^2];

//            sql = $"{sql} FROM \"{_tableNameValue}\"";

//            sql = $"{sql} {_joinOn}";

//            if (_whereExpressionParts.Count > 0 || _orWhereExpressionParts.Count > 0)
//                sql = $"{sql} WHERE";

//            if (_whereExpressionParts.Count > 0)
//            {
//                if (_rowNumValue != null)
//                    sql = $"{sql} ROWNUM {_comparison.GetOperator()} {_rowNumValue}";

//                if (allowSqlInjection)
//                    foreach (var where in _whereExpressionParts)
//                        sql = $"{sql} {where.ToSqlInjectionString()}";
//                else
//                    foreach (var where in _whereExpressionParts)
//                        sql = $"{sql} {where}";
//            }

//            if (_orWhereExpressionParts.Count > 0)
//            {
//                if (_whereExpressionParts.Count > 0)
//                    sql = $"{sql} AND (";

//                if (allowSqlInjection)
//                    foreach (var where in _orWhereExpressionParts)
//                        sql = $"{sql} {where.ToSqlInjectionString()}";
//                else
//                    foreach (var where in _orWhereExpressionParts)
//                        sql = $"{sql} {where}";

//                sql = sql[..^2];

//                if (_whereExpressionParts.Count > 0)
//                    sql = $"{sql})";
//            }

//            if (_orderByExpressionParts.Count > 0)
//            {
//                sql = $"{sql} ORDER BY";

//                foreach (var expressionPart in _orderByExpressionParts)
//                {
//                    RegisteredProperty<C> registeredProperty = RegisteredClass.RegisteredProperties.First(p => p.PropertyName == expressionPart.MemberExpression?.Member.Name);

//                    if (expressionPart.NodeType == ExpressionType.GreaterThan)
//                        sql = $"{sql} \"{registeredProperty.ColumnName}\" ASC";

//                    if (expressionPart.NodeType == ExpressionType.LessThan)
//                        sql = $"{sql} \"{registeredProperty.ColumnName}\" DESC";
//                }
//            }

//            if (_limitValue != null)
//                sql = $"{sql} LIMIT {_limitValue}";

//            return $"{sql};";
//        }

//        public async Task<IEnumerable<C>> QueryAsync() => await QueryAsync(QueryType.Select, ToString()).ConfigureAwait(false);

//        public async Task<C> QueryFirstAsync() => await QueryFirstAsync(QueryType.Select, ToString()).ConfigureAwait(false);

//        public async Task<C?> QueryFirstOrDefaultAsync() => await QueryFirstOrDefaultAsync(QueryType.Select, ToString()).ConfigureAwait(false);

//        public async Task<C> QuerySingleAsync() => await QuerySingleAsync(QueryType.Select, ToString()).ConfigureAwait(false);

//        public async Task<C?> QuerySingleOrDefaultAsync() => await QuerySingleOrDefaultAsync(QueryType.Select, ToString()).ConfigureAwait(false);

//        public async Task<List<C>> QueryToListAsync() => (await QueryAsync(QueryType.Select, ToString()).ConfigureAwait(false)).ToList();

//        public async Task<List<T>> QueryToListAsync<T>() where T : class
//        {
//            _results = await QueryAsync(QueryType.Select, ToString()).ConfigureAwait(false);

//            List<T> results = new List<T>();

//            foreach (var result in _results)
//            {
//                object recordObject = result;

//                T item = (T)recordObject;

//                results.Add(item);
//            }

//            return results;
//        }
//    }
//}
