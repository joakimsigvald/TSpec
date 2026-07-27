namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

/// <remarks>
/// <c>?.</c> and <c>.</c> both land here. Null-conditional access is how the spec
/// navigates to a value, not a claim about the subject, so the distinction is
/// dropped at parse time and every access renders with a plain dot.
/// </remarks>
internal sealed record Member(string Raw, Expr Target, string Name) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Target];
    public override string AsPath() => $"{Target.AsPath()}.{Name}";
    public override string ToSource() => $"{Target.ToSource()}.{Name}";
    public override Mention? AsMention() => Target.AsMention();
}