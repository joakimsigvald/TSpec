namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Call(string Raw, Expr Target, IReadOnlyList<Expr> Args) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => Args.Prepend(Target);

    /// The method/factory name being invoked, or null for non-named targets.
    public string? MethodName => Target switch
    {
        Identifier id => id.Name,
        Member m => m.Name,
        Generic { Target: Identifier gi } => gi.Name,
        Generic { Target: Member gm } => gm.Name,
        _ => null,
    };

    /// <c>default(T)</c> — Roslyn parses the keyword as a "literal", and
    /// applying it to a type ref turns it into a single-arg call we render
    /// as <c>"default T"</c>.
    public bool IsDefaultOf() => Target is Literal { Raw: "default" } && Args.Count == 1;

    /// A bare-identifier-with-args call like <c>One(model)</c> or
    /// <c>Add(a, b)</c>. Rendered as a natural-language phrase
    /// (<c>"one model"</c>, <c>"add a, b"</c>) — returns the identifier's
    /// name when this shape applies, null otherwise.
    public string? AsNaturalLanguageCall() =>
        Target is Identifier id && Args.Count >= 1 ? id.Name : null;

    /// If this call directly wraps a Mention factory (i.e. its Target is the
    /// <see cref="Generic"/> that produced the mention), its Args become the
    /// mention's constraints. Otherwise the inner mention is passed through.
    public override Mention? AsMention() => Target.AsMention() switch
    {
        null => null,
        var inner when Target is Generic
            => inner with { Boundary = Raw, Constraints = Args.Count > 0 ? Args : null },
        var inner => inner,
    };
}