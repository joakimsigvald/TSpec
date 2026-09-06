namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Generic(string Raw, Expr Target, IReadOnlyList<Expr> TypeArgs) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => TypeArgs.Prepend(Target);
    public override string AsPath() =>
        $"{Target.AsPath()}<{TypeArgText}>";

    public override string ToSource() => $"{Target.ToSource()}<{TypeArgText}>";

    internal string TypeArgText => string.Join(", ", TypeArgs.Select(t => t.Raw));

    public override Mention? AsMention() => MentionVerb is { } verb && TypeArgs.Count > 0
        ? new Mention(Raw, verb, string.Join(", ", TypeArgs.Select(t => t.Raw)), null)
        : null;

    /// Moq's <c>It.IsAny&lt;T&gt;()</c> reads as TSpec's <c>Any&lt;T&gt;()</c>: both mean any T.
    private string? MentionVerb => Target switch
    {
        Identifier id => id.Name,
        Member { Target: Identifier { Name: "It" }, Name: "IsAny" } => "Any",
        _ => null,
    };
}