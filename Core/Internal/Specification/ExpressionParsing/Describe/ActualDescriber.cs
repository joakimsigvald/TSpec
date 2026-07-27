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
            _ => $"{DescribeRoot(root)}.{Stitch(chain)}",
        };
    }

    private static (Anchor Kind, Expr Root) CollectChain(Expr expr, List<string> chain)
    {
        var cur = expr;
        while (true)
            switch (cur = cur.WithoutNoise())
            {
                case Member m when IsBindingWord(m.Name):
                    return (Anchor.BindingWord, m);
                case Member m:
                    chain.Add(m.Name);
                    cur = m.Target;
                    continue;
                case Call c when _ignoreBeforeResult.Contains(c.MethodName):
                    return (Anchor.ResultWrapper, c);
                case Call { Target: Member m } c:
                    chain.Add($"{m.Name}({string.Join(", ", c.Args.Select(a => a.Raw))})");
                    cur = m.Target;
                    continue;
                default:
                    return (Anchor.Expression, cur);
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
