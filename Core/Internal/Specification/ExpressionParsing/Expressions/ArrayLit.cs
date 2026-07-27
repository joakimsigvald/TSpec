namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record ArrayLit(string Raw, IReadOnlyList<Expr> Items) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => Items;
    public override string ToSource() => $"[{SourceList(Items)}]";
}