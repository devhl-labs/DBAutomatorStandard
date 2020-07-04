using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Text;

namespace Dapper.SqlWriter.Models
{

    public class BodyExpression<C> : IExpression where C : class
    {
        public BodyExpression(RegisteredProperty<C> registeredProperty, MemberExpression memberExpression, ExpressionType nodeType, ConstantExpression constantExpression, string prefix, int parameterName)
        {
            RegisteredProperty = registeredProperty;

            MemberExpression = memberExpression;

            NodeType = nodeType;

            ConstantExpression = constantExpression;

            Prefix = prefix;

            ParameterName = parameterName;
        }

        public RegisteredProperty<C> RegisteredProperty { get; }

        public MemberExpression MemberExpression { get; }

        public ExpressionType NodeType { get; }

        public ConstantExpression ConstantExpression { get; }

        public string Prefix { get; }

        public int ParameterName { get; }

        public override string ToString() => GetString() ?? $"\"{RegisteredProperty.RegisteredClass.DatabaseTableName}\".\"{RegisteredProperty.ColumnName}\" {NodeType.ToSqlSymbol()} @{Prefix}{ParameterName}";

        public string ToSqlInjectionString()
        {
            string? result = GetString();

            if (result != null) 
                return result;

            object? value = Utils.ToDatabaseColumn(RegisteredProperty, ConstantExpression.Value); // RegisteredProperty.ToDatabaseColumn(RegisteredProperty, ConstantExpression.Value);

            if (value?.GetType() == typeof(string))            
                return $"\"{RegisteredProperty.ColumnName}\" {NodeType.ToSqlSymbol()} '{value}'";            
            else            
                return $"\"{RegisteredProperty.ColumnName}\" {NodeType.ToSqlSymbol()} {value}";            
        }

        private string? GetString()
        {
            if (MemberExpression == null)            
                return NodeType.ToSqlSymbol();            

            if (ConstantExpression != null && ConstantExpression.Value == null && NodeType == ExpressionType.Equal) 
                return $"\"{RegisteredProperty.RegisteredClass.DatabaseTableName}\".\"{MemberExpression.Member.Name}\" IS NULL";

            if (ConstantExpression != null && ConstantExpression.Value == null && NodeType == ExpressionType.NotEqual) 
                return $"\"{RegisteredProperty.RegisteredClass.DatabaseTableName}\".\"{MemberExpression.Member.Name}\" IS NOT NULL";

            return null;
        }

        public string? GetSetString()
        {
            if (ConstantExpression?.Value != null && MemberExpression.Member.Name != null)
                return $"\"{RegisteredProperty.ColumnName}\" = @{Prefix}{ParameterName}";

            if (ConstantExpression != null && ConstantExpression.Value == null && NodeType == ExpressionType.Equal)
                return $"\"{RegisteredProperty.ColumnName}\" = NULL";

            throw new ArgumentException("Expression not handled");
        }
    }
}