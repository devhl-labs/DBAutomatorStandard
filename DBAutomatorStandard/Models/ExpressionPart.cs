using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace devhl.DBAutomator.Models
{
    public class ExpressionPart
    {
        public MemberExpression? MemberExpression { get; set; } = null;

        public ExpressionType NodeType { get; set; }

        public ConstantExpression? ConstantExpression { get; set; } = null;

    }
}
