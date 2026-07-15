namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record New(string Raw, string? TypeName, IReadOnlyList<Expr> Args, IReadOnlyList<Expr>? Init) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => Init is null ? Args : Args.Concat(Init);
}