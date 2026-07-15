namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Literal(string Raw) : Expr(Raw)
{
    public string Quoted => Requote(Raw);
}