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

    public abstract string Describe(Expr expr);

    protected static string DescribeAll(IEnumerable<Expr> exprs) =>
        string.Join(", ", exprs.Select(Value.Describe));

    /// Render TSpec's <c>A&lt;T&gt;</c> / <c>An&lt;T&gt;</c> / <c>The&lt;T&gt;</c>
    /// factory shapes, or null if <paramref name="expr"/> is no mention.
    protected static string? DescribeMention(Expr expr)
    {
        if (expr.AsMention() is not { } m)
            return null;

        string head = $"{m.Verb.AsWords()} {m.TypeArgs}";
        return m.Constraints is { Count: > 0 }
            ? $"{head} {{ {DescribeAll(m.Constraints)} }}"
            : DescribeWithDrilldown(head, expr.Raw, m.Boundary);
    }

    /// A member-access drilldown after the mention (<c>The&lt;Cart&gt;().Foo</c>)
    /// reads possessively: "the Cart's Foo". Any other suffix means the
    /// expression is more than a mention — not describable here (null).
    private static string? DescribeWithDrilldown(string head, string raw, string boundary)
    {
        if (raw.Length <= boundary.Length || !raw.StartsWith(boundary))
            return head;

        string suffix = raw[boundary.Length..].TrimStart().TrimStart('!');
        if (suffix.Length == 0)
            return head;

        return suffix.StartsWith('.') ? $"{head}'s {suffix[1..]}" : null;
    }

    protected static string DescribeNew(New n)
    {
        string head = NewHead(n);
        string init = n.Init is null ? "" : $" {{ {DescribeAll(n.Init)} }}";
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
        return omitArgs ? prefix : $"{prefix}({DescribeAll(n.Args)})";
    }
}
