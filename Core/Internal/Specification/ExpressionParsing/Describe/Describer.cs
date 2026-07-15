using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Describe;

/// <summary>
/// Base for the three description modes. Subclasses override
/// <see cref="Describe"/> for their mode-specific rendering, and inherit
/// Mention detection and <c>new</c>-expression rendering — both of which
/// always describe sub-expressions in value mode via <see cref="Value"/>.
/// </summary>
internal abstract class Describer
{
    /// Singleton value-mode describer, used wherever a sub-expression should
    /// be described as a value regardless of the outer context.
    public static readonly ValueDescriber Value = new();

    public abstract string Describe(Expr expr);

    protected static string DescribeAll(IEnumerable<Expr> exprs) =>
        string.Join(", ", exprs.Select(Value.Describe));

    /// Render TSpec's <c>A&lt;T&gt;</c> / <c>An&lt;T&gt;</c> / <c>The&lt;T&gt;</c>
    /// factory shapes. Three sub-cases: plain mention, with constraints, or
    /// with member-access drilldown. Detection lives on <see cref="Expr.AsMention"/>;
    /// this method shapes the result into text.
    protected static bool TryDescribeMention(Expr expr, out string description)
    {
        description = string.Empty;
        if (expr.AsMention() is not { } m) return false;

        string head = $"{m.Verb.AsWords()} {m.TypeArgs}";

        if (m.Constraints is { Count: > 0 })
        {
            description = $"{head} {{ {DescribeAll(m.Constraints)} }}";
            return true;
        }
        if (expr.Raw.Length > m.Boundary.Length && expr.Raw.StartsWith(m.Boundary))
        {
            string suffix = expr.Raw[m.Boundary.Length..].TrimStart().TrimStart('!');
            if (suffix.Length == 0)
            {
                description = head;
                return true;
            }
            if (!suffix.StartsWith('.')) 
                return false;
            description = $"{head}'s {suffix[1..]}";
            return true;
        }
        description = head;
        return true;
    }

    /// Render a <c>new</c> expression. With an init block, the user-written
    /// prefix is preserved verbatim (covers <c>new T()</c>, <c>new int[]</c>,
    /// <c>new T&lt;U&gt;()</c>, etc.); inits are always value-described.
    protected static string DescribeNew(New n)
    {
        string head = NewHead(n);
        string init = n.Init is null ? "" : $" {{ {DescribeAll(n.Init)} }}";
        return head + init;
    }

    /// The <c>new TypeName(args)</c> or <c>new TypeName</c> portion before any
    /// init block. When an init block is present, the user's literal text up to
    /// the <c>{</c> is preserved verbatim so <c>new T()</c>, <c>new int[]</c>,
    /// <c>new T&lt;U&gt;()</c> all render as written.
    private static string NewHead(New n)
    {
        if (n.Init is not null)
        {
            int braceIdx = n.Raw.IndexOf('{');
            if (braceIdx > 0) return n.Raw[..braceIdx].TrimEnd();
        }
        var prefix = string.IsNullOrEmpty(n.TypeName) ? "new" : $"new {n.TypeName}";
        bool omitArgs = n.Init is not null && n.Args.Count == 0;
        return omitArgs ? prefix : $"{prefix}({DescribeAll(n.Args)})";
    }
}