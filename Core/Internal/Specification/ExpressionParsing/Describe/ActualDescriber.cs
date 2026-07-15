using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Describe;

/// <summary>
/// Actual-mode description (used by <c>ParseActual</c>). Walks the rightmost
/// member-access chain to find the wrapping <c>Then(...)</c> / <c>And(...)</c>
/// call, then returns just the tail after that wrapper, prefixed by the
/// <paramref name="subject"/> the wrapper registered at runtime — the wrapper's
/// arguments are never interpreted here.
/// </summary>
internal sealed class ActualDescriber(string? subject = null) : Describer
{
    private static readonly string[] _ignoreBeforeResult = ["Then", "And", "Because"];
    private static readonly string[] _bindingWords = ["and", "that"];

    /// One step in the member/call chain, with the separator that precedes it.
    private sealed record Segment(string Name, bool NullConditional)
    {
        public string Separator => NullConditional ? "?." : ".";
    }

    public override string Describe(Expr expr)
    {
        var tail = new List<Segment>();
        var cur = expr;
        while (true)
        {
            if (cur is Member m)
            {
                // A binding-word continuation property (and, that) starts a
                // fresh value path: everything to its left is a previous step
                if (IsBindingWord(m.Name))
                    return Combine(null, Chain());
                tail.Add(new(m.Name, m.NullConditional));
                cur = m.Target;
                continue;
            }
            if (cur is not Call c)
                break;

            if (_ignoreBeforeResult.Contains(c.MethodName))
                return Combine(subject, Chain());

            if (c.Target is not Member memCall)
                break;

            tail.Add(new($"{memCall.Name}({string.Join(", ", c.Args.Select(a => a.Raw))})", memCall.NullConditional));
            cur = memCall.Target;
        }

        if (tail.Count == 0)
            return Value.Describe(expr);

        // Chains not anchored in Then/And keep the user's wording: the root
        // and call segments render the source verbatim, never value-described
        var root = cur is Identifier id ? id.Name : cur.Raw;
        var chain = Chain();
        return root + chain[0].Separator + Stitch(chain);

        List<Segment> Chain()
        {
            tail.Reverse(); // collected rightmost-first
            return tail;
        }
    }

    /// Connect the subject to the chain: an identifier joins the path with
    /// dots, while a prose subject (e.g. "the Checkout") reads possessively:
    /// "the Checkout's IsOpen".
    private static string Combine(string? subject, List<Segment> tail)
    {
        if (tail.Count == 0)
            return subject ?? string.Empty;
        if (string.IsNullOrEmpty(subject))
            return Stitch(tail);
        return IsIdentifier(subject)
            ? subject + tail[0].Separator + Stitch(tail)
            : $"{subject}'s {Stitch(tail)}";
    }

    private static string Stitch(List<Segment> tail)
        => tail[0].Name + string.Concat(tail.Skip(1).Select(s => s.Separator + s.Name));

    private static bool IsIdentifier(string s) => s.All(char.IsLetterOrDigit);

    private static bool IsBindingWord(string name)
        => _bindingWords.Contains(name, StringComparer.OrdinalIgnoreCase);
}
