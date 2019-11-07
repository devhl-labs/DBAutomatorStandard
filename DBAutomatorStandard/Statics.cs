using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBAutomatorLibrary
{
    internal static class Statics
    {
        public static string Left(this string param, int length)
        {
            string result = param.Substring(0, length);
            return result;
        }

        public static string Right(this string param, int length)
        {
            string result = param.Substring(param.Length - length, length);
            return result;
        }




        public static List<ConditionModel> GetConditions<T>(this Expression<Func<T, object>>? e)
        {
            var conditions = new List<ConditionModel>();

            if (e == null)
            {
                return conditions;
            }

            if (((UnaryExpression)e.Body).Operand is BinaryExpression binaryExpression)
            {
                CheckConditions(conditions, binaryExpression);
            }

            return conditions.OrderBy(c => c.Name).ThenBy(c => c.OperatorName).ToList();
        }

        private static void CheckConditions(List<ConditionModel> conditions, BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.AndAlso)
            {
                if(binaryExpression.Left is BinaryExpression left)
                {
                    CheckConditions(conditions, left);
                }
                
                if(binaryExpression.Right is BinaryExpression right)
                {
                    CheckConditions(conditions, right);
                }
            }
            else
            {
                conditions.Add(GetCondition(binaryExpression));
            }
        }

        private static ConditionModel GetCondition(BinaryExpression binaryExpression)
        {
            ConditionModel condition = new ConditionModel
            {
                Name = ((MemberExpression)binaryExpression.Left).Member.Name,
                Value = binaryExpression
                .Right.GetType().GetProperty("Value")
                .GetValue(binaryExpression.Right, null),

                OperatorName = binaryExpression.NodeType.ToString()
            };
            return condition;
        }

        public enum QueryType
        {
            Get
            , GetList
            , Delete
            , Update
        }






        //IDBObject events
        public static async Task OnLoaded<C>(C obj, DBAutomator dBAutomator)
        {
            if (obj is IDBObject dBObject)
            {
                dBObject.IsDirty = false;

                dBObject.IsNewRecord = false;

                await dBObject.OnLoaded(dBAutomator);
            }
        }

        public static async Task OnSave<C>(C obj, DBAutomator dBAutomator)
        {
            if (obj is IDBObject dBObject)
            {
                await dBObject.OnSave(dBAutomator);
            }
        }

        public static async Task OnSaved<C>(C obj, DBAutomator dBAutomator)
        {
            if (obj is IDBObject dBObject)
            {
                dBObject.IsDirty = false;

                dBObject.IsNewRecord = false;

                await dBObject.OnLoaded(dBAutomator);
            }
        }
    }
}
