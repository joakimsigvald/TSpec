namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Member(string Raw, Expr Target, string Name, bool NullConditional = false) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Target];
    public override string AsPath() => $"{Target.AsPath()}{(NullConditional ? "?" : "")}.{Name}";
    public override Mention? AsMention() => Target.AsMention();
}