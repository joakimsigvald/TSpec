namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record IndexExpr(string Raw, Expr Target, IReadOnlyList<Expr> Args) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => Args.Prepend(Target);
    public override string ToSource() => $"{Target.ToSource()}[{SourceList(Args)}]";
    public override Mention? AsMention() => Target.AsMention();
}