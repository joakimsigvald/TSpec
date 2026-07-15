namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

/// <summary>
/// Base for the parsed-expression tree. Each node knows its own
/// <see cref="Children"/> (for traversal) and how to render itself
/// <see cref="AsPath"/> (for dotted/generic path text). Domain-specific
/// rendering (mention lowering, lambda stripping, etc.) lives on the
/// describer family in the sibling <c>Describe</c> namespace.
/// </summary>
internal abstract record Expr(string Raw)
{
    public virtual IEnumerable<Expr> Children => [];
    public virtual string AsPath() => Raw;

    /// If this expression (or its outer wrappers) contains a Mention factory
    /// — <c>A&lt;T&gt;</c> / <c>An&lt;T&gt;</c> / <c>The&lt;T&gt;</c> etc. —
    /// describe its root, verb, type args, and any constraints. Otherwise null.
    public virtual Mention? AsMention() => null;

    /// Strip a leading <c>$</c>/<c>@</c> prefix and re-emit the quoted contents.
    protected static string Requote(string raw)
    {
        int q = raw.IndexOf('"');
        return q < 0 || raw.Length < 2 || raw[^1] != '"' ? raw : $"\"{raw[(q + 1)..^1]}\"";
    }
}