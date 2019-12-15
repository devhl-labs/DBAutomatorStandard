using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace devhl.DBAutomator
{
    public static class Statics
    {
        //public static object GetRootObject<T>(this Expression<Func<T, object>> expression)
        //{
        //    var propertyAccessExpression = expression.Body as MemberExpression;
        //    if (propertyAccessExpression == null)
        //        return null;

        //    //go up through property/field chain
        //    while (propertyAccessExpression.Expression is MemberExpression)
        //        propertyAccessExpression = (MemberExpression)propertyAccessExpression.Expression;

        //    //the last expression suppose to be a constant expression referring to captured variable ...
        //    var rootObjectConstantExpression = propertyAccessExpression.Expression as ConstantExpression;
        //    if (rootObjectConstantExpression == null)
        //        return null;

        //    //... which is stored in a field of generated class that holds all captured variables.
        //    var fieldInfo = propertyAccessExpression.Member as FieldInfo;
        //    if (fieldInfo != null)
        //        return fieldInfo.GetValue(rootObjectConstantExpression.Value);

        //    return null;
        //}





        //public static void PrintPropertyAndObject<T>(Expression<Func<T, object>> e)
        //{
        //    MemberExpression member = (MemberExpression)e.Body;
        //    Expression strExpr = member.Expression;
        //    if (strExpr.Type == typeof(String))
        //    {
        //        String str = Expression.Lambda<Func<String>>(strExpr).Compile()();
        //        Console.WriteLine("String: {0}", str);
        //    }
        //    string propertyName = member.Member.Name;
        //    T value = e.Compile()();
        //    Console.WriteLine("{0} : {1}", propertyName, value);
        //}




        //public static class LambdaVariableExtractor
        //{
            public static TValue ExtractValue<TValue>(
                this Delegate lambda, string variableName
            )
            {
                if (lambda == null)
                    throw new NullReferenceException("Empty lambda");

                var field = lambda.Method.DeclaringType?.GetFields
                    (
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.Static
                    )
                    .SingleOrDefault(x => x.Name == variableName);

                if (field == null)
                    throw new NullReferenceException("There isn't a variable with this name");

                if (field.FieldType != typeof(TValue))
                    throw new ArgumentException(
                        $"Wrong closure type. Expected - {typeof(TValue).FullName}. Actual - {field.FieldType.FullName}");

                return (TValue)field.GetValue(lambda.Target);
            }
        //}




















    }



}
