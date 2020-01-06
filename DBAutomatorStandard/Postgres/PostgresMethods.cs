using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;

using Dapper;

using MiaPlaza.ExpressionUtils;

using static devhl.DBAutomator.Statics;
using MiaPlaza.ExpressionUtils.Evaluating;

namespace devhl.DBAutomator
{
    internal static class PostgresMethods
    {
        public static bool IsStorable(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttributes<NotMappedAttribute>(true).FirstOrDefault() is NotMappedAttribute notMappedAttribute)
            {
                return false;
            }

            if (propertyInfo.PropertyType == typeof(string) ||
                propertyInfo.PropertyType == typeof(ulong) ||
                propertyInfo.PropertyType == typeof(ulong?) ||
                propertyInfo.PropertyType == typeof(bool) ||
                propertyInfo.PropertyType == typeof(bool?) ||
                propertyInfo.PropertyType == typeof(decimal) ||
                propertyInfo.PropertyType == typeof(decimal?) ||
                propertyInfo.PropertyType == typeof(DateTime) ||
                propertyInfo.PropertyType == typeof(DateTime?) ||
                propertyInfo.PropertyType == typeof(int) ||
                propertyInfo.PropertyType == typeof(int?) ||
                propertyInfo.PropertyType == typeof(long) ||
                propertyInfo.PropertyType == typeof(long?) ||

                propertyInfo.PropertyType.IsEnum
                )
            {
                return true;
            }
            return false;
        }

        public static BinaryExpression? GetBinaryExpression<C>(Expression<Func<C, object>>? where = null)
        {
            UnaryExpression? unaryExpression;

            BinaryExpression? binaryExpression = null;

            if (where != null)
            {
                unaryExpression = (UnaryExpression)where.Body;

                binaryExpression = (BinaryExpression)unaryExpression.Operand;
            }

            return binaryExpression;
        }

        public static Queue<string> GetOperator(string str)
        {
            Queue<string> result = new Queue<string>();

            for (int i = 0; i < str.Length; i++)
            {
                if (str.Length >= i + 10)
                {
                    if (str.Substring(i, ") OrElse (".Length) == ") OrElse (")
                    {
                        result.Enqueue("OR");
                    }
                }

                if (str.Length >= i + 11)
                {
                    if (str.Substring(i, ") AndAlso (".Length) == ") AndAlso (")
                    {
                        result.Enqueue("AND");
                    }
                }
            }

            return result;
        }

        public static string GetWhereClause<C>(this Expression<Func<C, object>> e, List<ExpressionModel<C>> expressions)
        {
            string result = string.Empty;

            var operators = GetOperator(e.ToString());

            foreach (var exp in expressions)
            {
                if (exp.Value != null && exp.Value.GetType() == typeof(string))
                {
                    result = $"{result} \"{exp.ColumnName}\" {exp.NodeType.GetMathSymbol().Replace("==", "=")} '{exp.Value}'";
                }
                else
                {
                    result = $"{result} \"{exp.ColumnName}\" {exp.NodeType.GetMathSymbol().Replace("==", "=")} {exp.Value}";
                }                

                if (operators.Count() > 0)
                {
                    result = $"{result} {operators.Dequeue()}";
                }
            }

            result = result.Replace(") OrElse (", ") OR (").Replace(") AndAlso (", ") AND (");

            return result;











            //if (e == null)
            //{
            //    return string.Empty;
            //}

            //if (e.Body is UnaryExpression unaryExpression && unaryExpression.Operand is BinaryExpression binaryExpression)
            //{
            //    string result = $"{binaryExpression.Left.ToString()} {binaryExpression.NodeType.GetMathSymbol()} {binaryExpression.Right.ToString()}";

            //    if (expressions != null)
            //    {
            //        foreach (var exp in expressions)
            //        {
            //            result = result.Replace(exp.ToString(), exp.ToSqlString());
            //        }
            //    }

            //    result = result.Replace(") OrElse (", ") OR (").Replace(") AndAlso (", ") AND (");

            //    return result;
            //}

            //return string.Empty;
        }

        public static string GetWhereClause<C>(C item, List<RegisteredProperty> registeredProperties, string delimiter = "AND")
        {
            if (item == null)
            {
                throw new NullReferenceException("The item cannot be null.");
            }

            string result = string.Empty;

            foreach (var property in registeredProperties)
            {
                result = $"{result} \"{property.ColumnName}\" = @{property.ColumnName} {delimiter}";
            }

            int remove = delimiter.Length + 1;

            result = result[0..^remove];

            return result;
        }

        public static void GetExpressions<C>(BinaryExpression? binaryExpression, List<ExpressionModel<C>> expressions, RegisteredClass<C> registeredClass, string parameterPrefix = "w_")
        {
            if (binaryExpression == null)
            {
                return;
            }

            if (binaryExpression.Left is BinaryExpression left)
            {
                GetExpressions(left, expressions, registeredClass);
            }

            if (binaryExpression.Right is BinaryExpression right)
            {
                GetExpressions(right, expressions, registeredClass);
            }

            ExpressionModel<C> expression = new ExpressionModel<C>(registeredClass, parameterPrefix)
            {
                NodeType = binaryExpression.NodeType
            };

            if (binaryExpression.Left is MemberExpression memberExpression)
            {
                //if (memberExpression.Expression is ConstantExpression c)
                //{
                //    Type t = c.Value.GetType();

                //    var x = t.InvokeMember(memberExpression.Member.Name, BindingFlags.GetField, null, c.Value, null);

                //    expression.Value = x;
                //}
                //else
                //{
                    if (memberExpression.Expression is ParameterExpression thisExpression)
                    {
                        expression.ParameterName = thisExpression.Name; //u
                    }

                    expression.PropertyName = memberExpression.Member.Name; //userid
                //}
            }
            else if (binaryExpression.Left is ConstantExpression constantExpression2)
            {
                Console.WriteLine(constantExpression2.Value);
            }
            else if (binaryExpression.Left is UnaryExpression unaryExpression)
            {
                Console.WriteLine(unaryExpression.Operand);
                Console.WriteLine(unaryExpression.Type);
                Console.WriteLine(unaryExpression.NodeType);
                
                if (unaryExpression.Operand is MemberExpression memberExpression1)
                {
                    if (memberExpression1.Expression is ParameterExpression thisExpression2)
                    {
                        expression.ParameterName = thisExpression2.Name;
                    }
                    expression.PropertyName = memberExpression1.Member.Name;
                }

            }

            var abc = binaryExpression.Left.GetType();

            if (binaryExpression.Right is ConstantExpression constantExpression)
            {
                expression.Value = constantExpression.Value;
            }
            else if (binaryExpression.Right is MethodCallExpression methodCallExpression)
            {
                var objectMember = Expression.Convert(methodCallExpression, typeof(object));

                var getterLambda = Expression.Lambda<Func<object>>(objectMember);

                expression.Value = getterLambda.Compile();
            }
            else if (binaryExpression.Right is MemberExpression memberExpression1)
            {
                //ConstantExpression a = (ConstantExpression) PartialEvaluator.PartialEval(memberExpression1, ExpressionInterpreter.Instance);

                //expression.Value = a.Value;

                if (memberExpression1.Expression is ConstantExpression c)
                {
                    //this works too!!!!!!!!!!!!!!!!!!!!!

                    //Type t = c.Value.GetType();

                    //var x = t.InvokeMember(memberExpression1.Member.Name, BindingFlags.GetField, null, c.Value, null);

                    //expression.Value = x;

                    ConstantExpression a = (ConstantExpression) PartialEvaluator.PartialEval(memberExpression1, ExpressionInterpreter.Instance);

                    expression.Value = a.Value;
                }
                else
                {
                    ConstantExpression a = (ConstantExpression) PartialEvaluator.PartialEval(memberExpression1, ExpressionInterpreter.Instance);

                    expression.Value = a.Value;
                }
            }
            else
            {
                Expression a = PartialEvaluator.PartialEval(binaryExpression.Right, ExpressionInterpreter.Instance);

                expression.Value = a.ToString(); // a.Value;

                //expression.Value = Expression.Lambda(binaryExpression.Right).Compile().DynamicInvoke();
            }

            if (expression.PropertyName != null)
            {
                expressions.Add(expression);
            }
        }

        public static DynamicParameters GetDynamicParametersFromExpression<C>(List<ExpressionModel<C>> expressions, DynamicParameters? p = null)
        {
            p ??= new DynamicParameters();

            foreach (var exp in expressions)
            {
                if (exp.Value is ulong || exp.Value is ulong?)
                {
                    p.Add($"{exp.ParameterPrefix}{exp.ColumnName}", Convert.ToInt64(exp.Value));
                }
                else
                {
                    p.Add($"{exp.ParameterPrefix}{exp.ColumnName}", exp.Value);
                }
            }

            return p;
        }

        public static DynamicParameters GetDynamicParameters<C>(C item, List<RegisteredProperty> registeredProperties)
        {
            if (item == null)
            {
                throw new NullReferenceException("The item cannot be null.");
            }

            DynamicParameters p = new DynamicParameters();

            foreach (var property in registeredProperties.Where(p => !p.IsAutoIncrement))
            {
                if (property.PropertyType == typeof(string))
                {
                    p.Add($"@{property.ColumnName}", item.GetType().GetProperty(property.PropertyName).GetValue(item, null));
                }
                else if (property.PropertyType == typeof(ulong))
                {
                    p.Add($"@{property.ColumnName}", Convert.ToInt64(item.GetType().GetProperty(property.PropertyName).GetValue(item, null)));
                }
                else if (property.PropertyType == typeof(ulong?))
                {
                    ulong? id = (ulong?) item.GetType().GetProperty(property.PropertyName).GetValue(item, null);

                    if (id == null)
                    {
                        p.Add($"@{property.ColumnName}", null);
                    }
                    else
                    {
                        p.Add($"@{property.ColumnName}", Convert.ToInt64(id));
                    }                    
                }
                else
                {
                    p.Add($"@{property.ColumnName}", item.GetType().GetProperty(property.PropertyName).GetValue(item));
                }
            }

            return p;
        }






        private static void GetParameterNames(BinaryExpression binaryExpression, List<string> parameterNames, List<string> columnNames, List<object> values)
        {
            if (binaryExpression.Left is BinaryExpression left)
            {
                GetParameterNames(left, parameterNames, columnNames, values);
            }

            if (binaryExpression.Right is BinaryExpression right)
            {
                GetParameterNames(right, parameterNames, columnNames, values);
            }

            if (binaryExpression.Left is MemberExpression memberExpression)
            {
                if (memberExpression.Expression is ParameterExpression thisExpression)
                {
                    if (!parameterNames.Any(p => p == thisExpression.Name))
                    {
                        parameterNames.Add(thisExpression.Name);
                    }
                }

                if (!columnNames.Any(c => c == memberExpression.Member.Name))
                {
                    columnNames.Add(memberExpression.Member.Name);
                }
            }

            if (binaryExpression.Right is ConstantExpression constantExpression)
            {
                values.Add(constantExpression.Value);
            }
        }

        private static string GetMathSymbol(this ExpressionType expression) =>
            expression switch
            {
                ExpressionType.Equal => "==",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.OrElse => "OR",
                ExpressionType.AndAlso => "AND",

                _ => throw new ArgumentException("Invalid enum value", nameof(expression))
            };










    }
}
