namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record TupleExpr(string Raw, IReadOnlyList<Expr> Items) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => Items;
}