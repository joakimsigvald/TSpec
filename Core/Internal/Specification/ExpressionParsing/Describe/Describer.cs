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
        string.Join($", {Wrap.Point}", exprs.Select(Item));

    /// An item of a list keeps the shape of a call, since a phrase's own commas would read as the list's.
    private static string Item(Expr expr)
        => expr.WithoutNoise() is Call call ? Link(call) : Value.Describe(expr);

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
    /// the dots of a plain path. The call left of such a joint is described, so its arguments read
    /// as values, but as a link in a call chain it keeps the shape of a call and never reads as a
    /// phrase.
    /// </summary>
    protected static string Path(Expr expr)
        => expr.WithoutNoise() is Member m && m.Target.WithoutNoise() is Call chained
            ? $"{Link(chained)}{Wrap.Point}.{m.Name}"
            : expr.AsPath();

    private static string Link(Call call)
        => call.AsNaturalLanguageCall() is not null && call.AsTagReference() is null
            ? $"{Path(call.Target)}{ArgList(call.Args)}"
            : Value.Describe(call);

    /// Render TSpec's <c>A&lt;T&gt;</c> / <c>An&lt;T&gt;</c> / <c>The&lt;T&gt;</c>
    /// factory shapes, or null if <paramref name="expr"/> is no mention.
    protected static string? DescribeMention(Expr expr)
    {
        if (expr.AsMention() is not { } m)
            return null;

        var typeArgs = m.TypeArgs.CountedBy(m.Verb);
        string head = $"{m.Verb.AsWords()} {typeArgs}";
        if (m.Constraints is not { Count: > 0 })
            return DescribeWithDrilldown(head, expr.Raw, m.Boundary, plural: typeArgs != m.TypeArgs);

        return AsMatchCondition(m) is { } condition
            ? $"{head}{Wrap.Enter} {Wrap.Point}where {Value.Describe(condition)}{Wrap.Exit}"
            : $"{head}{Braced(m.Constraints)}";
    }

    /// <summary>
    /// The condition of <c>Any&lt;T&gt;(constraint)</c>, which matches the values satisfying it: a method
    /// group, or a lambda's body. A lambda that assigns, copies with <c>with</c>, or has a block body sets
    /// up its value instead.
    /// </summary>
    private static Expr? AsMatchCondition(Mention m) => m switch
    {
        { Verb: "Any", Constraints: [Identifier or Member] } => m.Constraints[0],
        { Verb: "Any", Constraints: [Lambda { Params.Count: 1 } lambda] }
            when lambda.Body is not (Assign or With) && !lambda.Body.Raw.StartsWith('{') => lambda.Body,
        _ => null,
    };

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
        if (n.Init is not null && IsArrayCreation(n))
            return $"{n.TypeName}[{Wrap.Enter}{DescribeAll(n.Init)}{Wrap.Exit}]";

        string head = NewHead(n);
        string init = n.Init is null ? "" : Braced(n.Init);
        return head + init;
    }

    /// An array creation reads as the list it is, keeping the element type where one was written.
    private static bool IsArrayCreation(New n)
        => n.Args.Count == 0
        && n.Raw.IndexOf('{') is > 0 and var brace
        && Compact(n.Raw[..brace]) == Compact($"new {n.TypeName}[]");

    private static string Compact(string text) => text.Replace(" ", "");

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
