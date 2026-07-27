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

    /// Peel operators that are pure language mechanism — they say how the code
    /// runs, never what the subject does, so no description mode should see them.
    public virtual Expr WithoutNoise() => this;

    /// The expression as C#, rebuilt from the tree so that erased operators
    /// cannot survive inside a parent's <see cref="Raw"/>. Unlike the describers
    /// this invents nothing — no mentions, no prose, no elided receivers — and
    /// spacing is the printer's, not the source's. Used where a description has
    /// no better rendering than the code itself.
    public virtual string ToSource() => Raw;

    protected static string SourceList(IEnumerable<Expr> exprs)
        => string.Join(", ", exprs.Select(e => e.ToSource()));

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