using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.SqlWriter.Models
{
    public class ExpressionPart
    {
        public MemberExpression? MemberExpression { get; set; } = null;

        public ExpressionType NodeType { get; set; }

        public ConstantExpression? ConstantExpression { get; set; } = null;

    }
}
