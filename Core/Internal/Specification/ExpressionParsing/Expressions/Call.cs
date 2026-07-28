namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Call(string Raw, Expr Target, IReadOnlyList<Expr> Args) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => Args.Prepend(Target);
    public override string ToSource() => $"{Target.ToSource()}({SourceList(Args)})";

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

    /// <summary>
    /// A tag reference — <c>The(_roomNumber)</c> — returning the tag's variable, or null. The shape
    /// is decisive: <c>The</c> has exactly one overload taking an argument, and it takes a
    /// <c>Tag&lt;T&gt;</c>, so a single bare identifier passed to it is always a tag.
    /// </summary>
    public string? AsTagReference()
        => Target is Identifier { Name: "The" } && Args is [Identifier tag] ? tag.Name : null;

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