using System.Linq.Expressions;

namespace Dapper.SqlWriter.Models
{
    public class NodeExpression : IExpression
    {
        public NodeExpression(ExpressionType nodeType)
        {
            NodeType = nodeType;
        }

        public ExpressionType NodeType { get; }

        public override string ToString()
        {
            return NodeType.ToSqlSymbol();
        }

        public string ToSqlInjectionString() => ToString();
    }
}