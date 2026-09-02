using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Describe;

/// <summary>
/// Actual-mode description (used by <c>DescribeActual</c>). Walks the rightmost
/// member-access chain to find the wrapping <c>Then(...)</c> / <c>And(...)</c>
/// call, then returns just the tail after that wrapper, prefixed by the
/// <paramref name="subject"/> the wrapper registered at runtime — the wrapper's
/// arguments are never interpreted here.
/// </summary>
internal sealed class ActualDescriber(string? subject = null) : Describer
{
    private static readonly string[] _ignoreBeforeResult = ["Then", "And", "Because"];
    private static readonly string[] _bindingWords = ["and", "that"];

    /// What the collected chain is anchored in, at its left end.
    private enum Anchor
    {
        /// A plain expression, rendered as the chain's root.
        Expression,
        /// A Then/And/Because wrapper call, replaced by the registered subject.
        ResultWrapper,
        /// A binding-word continuation property (and, that) — everything left
        /// of it belongs to a previous step.
        BindingWord,
    }

    protected override string Render(Expr expr)
    {
        var chain = new List<string>();
        var (anchor, root) = CollectChain(expr, chain);
        chain.Reverse(); // collected rightmost-first

        return anchor switch
        {
            Anchor.BindingWord => Combine(null, chain),
            Anchor.ResultWrapper => Combine(subject, chain),
            _ when chain.Count == 0 => Value.Describe(expr),
            // Chains not anchored in Then/And keep the user's wording: the root
            // and call segments render the source verbatim, never value-described
            _ => $"{root}.{Stitch(chain)}",
        };
    }

    /// <summary>
    /// Walks the chain right to left, collecting its segments. What comes back with it is the root
    /// those segments hang off — empty where an anchor stands in its place, since the subject is
    /// then what they hang off instead.
    /// </summary>
    private static (Anchor Kind, string Root) CollectChain(Expr expr, List<string> chain)
    {
        var cur = expr;
        // An indexer is no segment of its own — it belongs to whatever it indexes, which the walk
        // reaches later. So it waits here, to the right of the segment it will be written onto,
        // and where the walk ends before reaching one, the root is what it was indexing.
        var indexers = string.Empty;
        while (true)
            switch (cur = cur.WithoutNoise())
            {
                case Member m when IsBindingWord(m.Name):
                    return (Anchor.BindingWord, string.Empty);
                case Member m:
                    chain.Add(m.Name + indexers);
                    indexers = string.Empty;
                    cur = m.Target;
                    continue;
                case IndexExpr x:
                    indexers = $"[{string.Join(", ", x.Args.Select(a => a.Raw))}]{indexers}";
                    cur = x.Target;
                    continue;
                case Call c when _ignoreBeforeResult.Contains(c.MethodName):
                    return (Anchor.ResultWrapper, string.Empty);
                case Call c when TryCallee(c, out var callee, out var calledOn):
                    chain.Add($"{callee}({string.Join(", ", c.Args.Select(a => a.Raw))}){indexers}");
                    indexers = string.Empty;
                    cur = calledOn;
                    continue;
                default:
                    return (Anchor.Expression, DescribeRoot(cur) + indexers);
            }
    }

    /// <summary>
    /// The name a call invokes and what it is called on — false where it is called on something
    /// unnamed, which the chain cannot hold. Type arguments spelled out at the call site are part
    /// of that name: the chain says what the reader wrote, and the reader wrote them.
    /// </summary>
    private static bool TryCallee(Call call, out string callee, out Expr calledOn)
    {
        callee = string.Empty;
        calledOn = call;
        switch (call.Target)
        {
            case Member m:
                callee = m.Name;
                calledOn = m.Target;
                return true;
            case Generic { Target: Member m } g:
                callee = $"{m.Name}<{g.TypeArgText}>";
                calledOn = m.Target;
                return true;
            default:
                return false;
        }
    }

    private static string DescribeRoot(Expr root) => root is Identifier id ? id.Name : root.Raw;

    /// Connect the subject to the chain: an identifier joins the path with
    /// dots, while a prose subject (e.g. "the Checkout") reads possessively:
    /// "the Checkout's IsOpen".
    private static string Combine(string? subject, List<string> chain)
    {
        if (chain.Count == 0)
            return subject ?? string.Empty;
        if (string.IsNullOrEmpty(subject))
            return Stitch(chain);
        return IsIdentifier(subject)
            ? $"{subject}.{Stitch(chain)}"
            : $"{subject}'s {Stitch(chain)}";
    }

    private static string Stitch(List<string> chain) => string.Join(".", chain);

    private static bool IsIdentifier(string s) => s.All(char.IsLetterOrDigit);

    private static bool IsBindingWord(string name)
        => _bindingWords.Contains(name, StringComparer.OrdinalIgnoreCase);
}
