namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record InterpolatedString(string Raw) : Expr(Raw)
{
    public string Quoted => Requote(Raw);
}