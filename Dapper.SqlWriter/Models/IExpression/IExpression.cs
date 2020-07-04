namespace Dapper.SqlWriter.Models
{
    public interface IExpression 
    {
        string ToSqlInjectionString();
    }
}