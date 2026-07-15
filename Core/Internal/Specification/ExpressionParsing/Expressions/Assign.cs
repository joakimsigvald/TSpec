namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Assign(string Raw, string Op, Expr Target, Expr Value) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Target, Value];
}