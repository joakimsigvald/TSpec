using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Describe;

/// <summary>
/// Base for the three description modes. Subclasses override
/// <see cref="Describe"/> for their mode-specific rendering; sub-expressions
/// are always described in value mode via <see cref="Value"/>.
/// </summary>
internal abstract class Describer
{
    public static readonly ValueDescriber Value = new();

    /// Noise is peeled here rather than in each mode, so a single predicate
    /// decides what never reaches a specification.
    public string Describe(Expr expr) => Render(expr.WithoutNoise());

    protected abstract string Render(Expr expr);

    protected static string DescribeAll(IEnumerable<Expr> exprs) =>
        string.Join($", {Wrap.Point}", exprs.Select(Value.Describe));

    /// An argument list is a nested construct: after the opening paren and after each comma the
    /// remainder may move to a continuation line, ranked one below the construct the call sits in.
    protected static string ArgList(IReadOnlyList<Expr> args)
        => args.Count == 0 ? "()" : $"({Wrap.Enter}{Wrap.Point}{DescribeAll(args)}{Wrap.Exit})";

    /// A brace block prefers moving whole to a continuation line — the point before the brace —
    /// over breaking inside it, and its members rank one level deeper still.
    protected static string Braced(IEnumerable<Expr> init)
        => $"{Wrap.Enter} {Wrap.Point}{{ {Wrap.Enter}{DescribeAll(init)}{Wrap.Exit} }}{Wrap.Exit}";

    /// <summary>
    /// A dotted path — with a break point at each joint where the dot connects two calls, never at
    /// the dots of a plain path. The call left of such a joint is a value like any other, so it is
    /// described rather than quoted: its arguments read as prose.
    /// </summary>
    protected static string Path(Expr expr)
        => expr.WithoutNoise() is Member m && m.Target.WithoutNoise() is Call chained
            ? $"{Value.Describe(chained)}{Wrap.Point}.{m.Name}"
            : expr.AsPath();

    /// Render TSpec's <c>A&lt;T&gt;</c> / <c>An&lt;T&gt;</c> / <c>The&lt;T&gt;</c>
    /// factory shapes, or null if <paramref name="expr"/> is no mention.
    protected static string? DescribeMention(Expr expr)
    {
        if (expr.AsMention() is not { } m)
            return null;

        var typeArgs = m.TypeArgs.CountedBy(m.Verb);
        string head = $"{m.Verb.AsWords()} {typeArgs}";
        return m.Constraints is { Count: > 0 }
            ? $"{head}{Braced(m.Constraints)}"
            : DescribeWithDrilldown(head, expr.Raw, m.Boundary, plural: typeArgs != m.TypeArgs);
    }

    /// <summary>
    /// A member-access drilldown after the mention (<c>The&lt;Cart&gt;().Foo</c>) reads possessively:
    /// "the Cart's Foo". Any other suffix means the expression is more than a mention — not
    /// describable here (null).
    /// </summary>
    /// <remarks>
    /// A plural takes the bare apostrophe, so a count that made the type read as "MyModels" does not
    /// then write "MyModels's".
    /// </remarks>
    private static string? DescribeWithDrilldown(string head, string raw, string boundary, bool plural)
    {
        if (raw.Length <= boundary.Length || !raw.StartsWith(boundary))
            return head;

        string suffix = raw[boundary.Length..].TrimStart().TrimStart('!');
        if (suffix.Length == 0)
            return head;

        return suffix.StartsWith('.') ? $"{head}'{(plural ? "" : "s")} {suffix[1..]}" : null;
    }

    protected static string DescribeNew(New n)
    {
        string head = NewHead(n);
        string init = n.Init is null ? "" : Braced(n.Init);
        return head + init;
    }

    /// When an init block is present, the user's literal text up to the
    /// <c>{</c> is preserved verbatim so <c>new T()</c>, <c>new int[]</c>,
    /// <c>new T&lt;U&gt;()</c> all render as written.
    private static string NewHead(New n)
    {
        if (n.Init is not null)
        {
            int braceIdx = n.Raw.IndexOf('{');
            if (braceIdx > 0)
                return n.Raw[..braceIdx].TrimEnd();
        }
        var prefix = string.IsNullOrEmpty(n.TypeName) ? "new" : $"new {n.TypeName}";
        bool omitArgs = n.Init is not null && n.Args.Count == 0;
        return omitArgs ? prefix : $"{prefix}{ArgList(n.Args)}";
    }
}
