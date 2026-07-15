namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Binary(string Raw, string Op, Expr Left, Expr Right) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Left, Right];
}