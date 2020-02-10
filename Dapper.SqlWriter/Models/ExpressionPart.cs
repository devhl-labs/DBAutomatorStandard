using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.SqlWriter.Models
{
    public class ExpressionPart
    {
        public MemberExpression? MemberExpression { get; set; } = null;

        public ExpressionType? NodeType { get; set; } = null;

        public ConstantExpression? ConstantExpression { get; set; } = null;

        public string? ConstantVariable { get; set; } = null;

        public Parens? Parens { get; set; } = null;

        public override string ToString()
        {
            string? result = ToPartialString();

            if (result != null) return result;

            return $"\"{MemberExpression.Member.Name}\" {NodeType.ToSqlSymbol()} @{ConstantVariable}";
        }

        public string ToSqlInjectionString()
        {
            string? result = ToPartialString();

            if (result != null) return result;

            return $"\"{MemberExpression.Member.Name}\" {NodeType.ToSqlSymbol()} '{ConstantExpression.Value}'";
        }

        private string? ToPartialString()
        {
            if (Parens == Dapper.SqlWriter.Parens.Left) return "(";

            if (Parens == Dapper.SqlWriter.Parens.Right) return ")";

            if (MemberExpression == null)
            {
                if (NodeType == null) return string.Empty;

                return NodeType.ToSqlSymbol();
            }

            if (ConstantExpression != null && ConstantExpression.Value == null && NodeType == ExpressionType.Equal) return $"\"{MemberExpression.Member.Name}\" IS NULL";

            if (ConstantExpression != null && ConstantExpression.Value == null && NodeType == ExpressionType.NotEqual) return $"\"{MemberExpression.Member.Name}\" IS NOT NULL";

            return null;
        }
    }
}
