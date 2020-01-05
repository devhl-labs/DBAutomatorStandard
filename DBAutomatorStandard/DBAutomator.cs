using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MiaPlaza.ExpressionUtils;
using MiaPlaza.ExpressionUtils.Evaluating;
using Microsoft.Extensions.Logging;

using static devhl.DBAutomator.Enums;

namespace devhl.DBAutomator
{
    public delegate void IsAvailableChangedEventHandler(bool isAvailable);

    public delegate void SlowQueryWarningEventHandler(string methodName, TimeSpan timeSpan);

    public class DBAutomator
    {
        public event SlowQueryWarningEventHandler? OnSlowQueryDetected;

        public ILogger? Logger { get; set; }

        private const string _source = nameof(DBAutomator);

        public readonly List<RegisteredClass> RegisteredClasses = new List<RegisteredClass>();

        public QueryOptions QueryOptions { get; set; }

        public DBAutomator(QueryOptions queryOptions, ILogger? logger = null)
        {
            Logger = logger;

            QueryOptions = queryOptions;
        }

#nullable disable
        public DBAutomator()
        {
            
        }
#nullable enable

        public void Initialize(QueryOptions queryOptions, ILogger? logger = null)
        {
            Logger = logger;

            QueryOptions = queryOptions;
        }

        public RegisteredClass Register(object someObject)
        {
            try
            {
                var registeredClass = new RegisteredClass(someObject);

                RegisteredClasses.Add(registeredClass);

                return registeredClass;
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        //public void Register<C>()
        //{
        //    try
        //    {
        //        var registeredClass = new RegisteredClass<C>();

        //        RegisteredClasses.Add(registeredClass);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new DbAutomatorException(e.Message, e);
        //    }
        //}


        internal void SlowQueryDetected(string methodName, TimeSpan timeSpan)
        {
            OnSlowQueryDetected?.Invoke(methodName, timeSpan);

            Logger?.LogWarning(LoggingEvents.SlowQuery, "{source}: Slow Query {methodName} took {seconds} seconds.", _source, methodName, (int)timeSpan.TotalSeconds);
        }

        public void Test(Expression exp)
        {
            if (exp is BinaryExpression binaryExpression)
            {
                if (binaryExpression.Left is BinaryExpression leftBinary) Test(leftBinary);

                if (binaryExpression.Right is BinaryExpression rightBinary) Test(rightBinary);

                if (binaryExpression.Left is UnaryExpression leftUnary)
                {
                    Console.WriteLine(leftUnary.ToString());
                }
                else if (binaryExpression.Left is ConstantExpression constantLeft)
                {
                    ConstantExpression left = (ConstantExpression) PartialEvaluator.PartialEval(constantLeft, ExpressionInterpreter.Instance);

                    Console.WriteLine(left);
                }
                else if (binaryExpression.Left is MemberExpression memberExpression)
                {
                    Console.WriteLine(memberExpression.ToString());
                }
                else
                {
                    var type = binaryExpression.Left.GetType();

                    Console.WriteLine(binaryExpression.Left.ToString());
                }
            }



            //if (exp is BinaryExpression binaryExpression && binaryExpression.Left is BinaryExpression leftBinary)
            //{
            //    Test(leftBinary);
            //}
            //else if (exp is BinaryExpression binaryExpression2 && binaryExpression.Left is BinaryExpression rightBinary)
            //{
            //    Test(unaryExpression);
            //}
            //else if ((exp is ConditionalExpression conditionalExpression))
            //{
            //    Test(conditionalExpression);
            //}
            //else
            //{
            //    BinaryExpression b = (BinaryExpression) exp;

            //if (binaryExpression.Left is binaryExpressioninaryExpression e)
            //{
            //    Test(e);
            //}
            //else
            //{
                //if (binaryExpression.Left is UnaryExpression leftUnary)
                //{
                //    Console.WriteLine(leftUnary.ToString());
                //}
                //else if (binaryExpression.Left is ConstantExpression constantLeft)
                //{
                //    ConstantExpression left = (ConstantExpression) PartialEvaluator.PartialEval(constantLeft, ExpressionInterpreter.Instance);

                //    Console.WriteLine(left);
                //}
                //else
                //{
                //    var type = binaryExpression.Left.GetType();

                //    Console.WriteLine(binaryExpression.Left.ToString());
                //}

            //}


            //if (binaryExpression.Right is binaryExpressioninaryExpression rightbinaryExpressioninary)
            //{
            //    Test(rightbinaryExpressioninary);
            //}
            //else
            //{
            //    if (binaryExpression.Right is ConstantExpression constantRight)
            //    {
            //        ConstantExpression left = (ConstantExpression) PartialEvaluator.PartialEval(constantRight, ExpressionInterpreter.Instance);

            //        Console.WriteLine(left);
            //    }
            //    else
            //    {
            //        if (binaryExpression.Right is ConstantExpression constantExp)
            //        {
            //            Console.WriteLine(binaryExpression.Right.ToString());
            //        }
            //        else
            //        {
            //            Expression pe = PartialEvaluator.PartialEval(binaryExpression.Right, ExpressionInterpreter.Instance);
            //        }
            //    }
            //}
        //}
        }


        public async Task<IEnumerable<C>> GetAsync<C>(Expression<Func<C, object>>? where = null, OrderByClause<C>? orderBy = null, QueryOptions? queryOptions = null)
        {
            try
            {
                try
                {
                    if (where != null)
                    {
                        Expression<Func<C, object>> exp = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

                        where = exp;

                        //if (exp is UnaryExpression un)
                        //{
                        //var unaryExpression = (UnaryExpression) where.Body;

                        //var binaryExpression = (BinaryExpression) unaryExpression.Operand;

                        //Test(binaryExpression);
                        //}

                        //Expression<Func<C, object>> test4 = PartialEvaluator.PartialEvalBody(where, ExpressionInterpreter.Instance);

                        //Console.WriteLine(test4.ToString());

                        //var test5 = test4.GetType();

                        //UnaryExpression? unaryExpression;

                        //BinaryExpression? binaryExpression = null;

                        //if (test4 != null)
                        //{
                        //    unaryExpression = (UnaryExpression) test4.Body;

                        //    binaryExpression = (BinaryExpression) unaryExpression.Operand;

                        //    var test6 = binaryExpression.Right.GetType();

                        //    //var test7 = binaryExpression.Left.get
                        //}

                        //var unaryExpression = (UnaryExpression) where.Body;

                        //var binaryExpression = (BinaryExpression) unaryExpression.Operand;

                        //Test(binaryExpression);
                    }
                }
                catch (Exception)
                {

                    //throw;
                }

                queryOptions ??= QueryOptions;

                ISelectQuery<C> query;

                if (queryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresSelectQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.GetAsync(where, orderBy).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<C> GetFirstOrDefaultAsync<C>(Expression<Func<C, object>>? where = null, OrderByClause<C>? orderBy = null, QueryOptions? queryOptions = null)
        {
            try
            {
                var result = await GetAsync(where, orderBy, queryOptions).ConfigureAwait(false);

                return result.ToList().FirstOrDefault();               

            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<C> InsertAsync<C>(C item, QueryOptions? queryOptions = null)
        {
            try
            {
                if (item == null)
                {
                    throw new NullReferenceException("The item cannot be null.");
                }

                queryOptions ??= QueryOptions;

                IInsertQuery<C> query;

                if (queryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresInsertQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.InsertAsync(item).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<IEnumerable<C>> DeleteAsync<C>(Expression<Func<C, object>>? where = null, QueryOptions? queryOptions = null)
        {
            try
            {
                queryOptions ??= QueryOptions;

                IDeleteQuery<C> query;

                if (queryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresDeleteQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.DeleteAsync(where).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<int> DeleteAsync<C>(C item, QueryOptions? queryOptions = null)
        {
            try
            {
                queryOptions ??= QueryOptions;

                IDeleteQuery<C> query;

                if (QueryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresDeleteQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.DeleteAsync(item).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<IEnumerable<C>> UpdateAsync<C>(Expression<Func<C, object>> set, Expression<Func<C, object>>? where = null, QueryOptions? queryOptions = null)
        {
            try
            {
                queryOptions ??= QueryOptions;

                IUpdateQuery<C> query;

                if (queryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresUpdateQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.UpdateAsync(set, where).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }

        public async Task<C> UpdateAsync<C>(C item, QueryOptions? queryOptions = null)
        {
            try
            {
                queryOptions ??= QueryOptions;

                IUpdateQuery<C> query;

                if (queryOptions.DataStore == DataStore.PostgreSQL)
                {
                    query = new PostgresUpdateQuery<C>(this, queryOptions, Logger);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return await query.UpdateAsync(item).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new DbAutomatorException(e.Message, e);
            }
        }
    }
}
