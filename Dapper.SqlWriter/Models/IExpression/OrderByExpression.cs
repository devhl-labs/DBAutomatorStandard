using System.Linq.Expressions;

namespace Dapper.SqlWriter.Models
{
    public class OrderByExpression<C> : IExpression where C : class
    {
        public OrderByExpression(RegisteredProperty<C> registeredProperty, MemberExpression memberExpression, ExpressionType nodeType)
        {
            RegisteredProperty = registeredProperty;

            MemberExpression = memberExpression;

            NodeType = nodeType;
        }

        public RegisteredProperty<C> RegisteredProperty { get; }

        public MemberExpression MemberExpression { get; }

        public ExpressionType NodeType { get; }

        public string ToSqlInjectionString() => ToString();

        public override string ToString()
        {
            if (NodeType == ExpressionType.LessThan)
                return $"\"{RegisteredProperty.ColumnName}\" DESC";

            return $"\"{RegisteredProperty.ColumnName}\" ASC";
        }
    }
}