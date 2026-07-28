namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

/// <summary>
/// An interpolated string, split into literal chunks and <see cref="Interpolation"/> holes so that
/// what a hole says can be described like any other expression. Without that split, the inside of a
/// quoted string would be the one place a specification still showed source code.
/// </summary>
/// <remarks>
/// <c>Open</c> is the opening delimiter as written, <c>$"</c> or <c>$@"</c>. The description drops
/// it; the source keeps it.
/// </remarks>
internal sealed record InterpolatedString(string Raw, string Open, IReadOnlyList<Expr> Parts) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => Parts;

    public override string ToSource() => $"{Open}{Body(hole => hole.ToSource())}\"";

    /// The string as a specification reads it: an ordinary quoted string whose holes have been
    /// described by <paramref name="describeHole"/>.
    internal string Quoted(Func<Expr, string> describeHole) => $"\"{Body(describeHole)}\"";

    private string Body(Func<Expr, string> renderHole)
        => string.Concat(Parts.Select(part => part switch
        {
            Interpolation hole => $"{{{renderHole(hole.Value)}{hole.Suffix}}}",
            _ => part.Raw,
        }));
}
