namespace Dapper.SqlWriter.Models
{
    public class ParenthesisExpression : IExpression
    {
        public ParenthesisExpression(Parens parens)
        {
            Parens = parens;
        }

        public Parens? Parens { get; }

        public override string ToString()
        {
            if (Parens == Dapper.SqlWriter.Parens.Left)
                return "(";

            return ")";
        }

        public string ToSqlInjectionString() => ToString();
    }
}