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

    private string? MentionVerb => Target is Identifier id ? id.Name : null;
}