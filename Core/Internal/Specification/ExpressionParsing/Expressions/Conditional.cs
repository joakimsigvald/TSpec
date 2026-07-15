namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Conditional(string Raw, Expr Cond, Expr Then, Expr Else) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Cond, Then, Else];
}