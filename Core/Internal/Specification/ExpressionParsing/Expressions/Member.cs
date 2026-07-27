namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Member(string Raw, Expr Target, string Name, bool NullConditional = false) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Target];
    public override string AsPath() => $"{Target.AsPath()}{Separator}{Name}";
    public override string ToSource() => $"{Target.ToSource()}{Separator}{Name}";

    private string Separator => NullConditional ? "?." : ".";
    public override Mention? AsMention() => Target.AsMention();
}