using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.SqlWriter.Models
{
    public class ExpressionPart<C> where C : class
    {
        public RegisteredProperty<C>? RegisteredProperty { get; set; } = null;

        public MemberExpression? MemberExpression { get; set; } = null;

        public ExpressionType? NodeType { get; set; } = null;

        public ConstantExpression? ConstantExpression { get; set; } = null;

        public string? ConstantVariable { get; set; } = null;

        public Parens? Parens { get; set; } = null;

        public string? Prefix { get; set; } = null;

        public int? ParameterName { get; set; } = null;

        public override string ToString() => GetString() ?? $"\"{RegisteredProperty.ColumnName}\" {NodeType.ToSqlSymbol()} @{Prefix}{ParameterName}";

        public string ToSqlInjectionString()
        {
            string? result = GetString();

            if (result != null) return result;

            object? value = RegisteredProperty.ToDatabaseColumn(RegisteredProperty, ConstantExpression.Value);

            if (value.GetType() == typeof(string))
            {
                return $"\"{RegisteredProperty.ColumnName}\" {NodeType.ToSqlSymbol()} '{value}'";
            }
            else
            {
                return $"\"{RegisteredProperty.ColumnName}\" {NodeType.ToSqlSymbol()} {value}";
            }
        }

        private string? GetString()
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

        public string? GetSetString()
        {
            if (MemberExpression == null)
                return null;

            if (ConstantExpression?.Value != null && MemberExpression.Member.Name != null)
                return $"\"{MemberExpression.Member.Name}\" = @{Prefix}{ParameterName}";

            if (ConstantExpression != null && ConstantExpression.Value == null && NodeType == ExpressionType.Equal)
                return $"\"{MemberExpression.Member.Name}\" = NULL";

            throw new ArgumentException("Expression not handled");
        }
    }
}
