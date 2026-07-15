namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record With(string Raw, Expr Target, IReadOnlyList<Expr> Init) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => Init.Prepend(Target);
}