using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace DBAutomatorStandard
{
    public class ExpressionModel<C>
    {
        private RegisteredClass _registeredClass;

        public string ParameterPrefix { get; }

        public ExpressionModel(RegisteredClass registeredClass, string parameterPrefix = "w_")
        {
            _registeredClass = registeredClass;
            ParameterPrefix = parameterPrefix;
        }

        public ExpressionType NodeType { get; set; }

        public string? ParameterName { get; set; }

        private string? _propertyName;
        
        public string? PropertyName
        {
            get
            {
                return _propertyName;
            }
        
            set
            {
                _propertyName = value;

                ColumnName = _registeredClass.RegisteredProperties.First(p => p.PropertyName == _propertyName).ColumnName;
            }
        }

        public object? Value { get; set; }

        public string? ColumnName { get; set; }

        public string NodeTypeSqlString() =>
            NodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                

                _ => throw new ArgumentException("Invalid enum value", nameof(NodeType))
            };

        public string NodeTypeExpressionString() =>
    NodeType switch
    {
        ExpressionType.Equal => "==",
        ExpressionType.GreaterThan => ">",
        ExpressionType.GreaterThanOrEqual => ">=",
        ExpressionType.LessThan => "<",
        ExpressionType.LessThanOrEqual => "<=",


        _ => throw new ArgumentException("Invalid enum value", nameof(NodeType))
    };

        public override string ToString()
        {
            if (Value is string)
            {
                return $"{ParameterName}.{PropertyName} {NodeTypeExpressionString()} \"{Value}\"";
            }
            else
            {
                return $"{ParameterName}.{PropertyName} {NodeTypeExpressionString()} {Value}";
            }
        }

        public string ToSqlString()
        {
            return $"\"{ColumnName}\" {NodeTypeSqlString()} @{ParameterPrefix}{ColumnName}";
        }
    }
}
