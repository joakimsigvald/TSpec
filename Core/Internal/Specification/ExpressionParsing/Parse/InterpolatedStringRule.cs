using System.Text;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// Splits an interpolated string literal into its literal chunks and its <c>{…}</c> holes, parsing
/// each hole as an expression in its own right. A hole that will not parse comes back as
/// <see cref="Unknown"/> and therefore renders exactly as it was written, so a shape this rule does
/// not understand costs nothing.
/// </summary>
internal static class InterpolatedStringRule
{
    public static Expr Parse(string raw)
    {
        int open = raw.IndexOf('"');
        if (open < 0 || raw.Length < 2 || raw[^1] != '"')
            return new InterpolatedString(raw, raw, [new Literal(raw)]);

        return new InterpolatedString(raw, raw[..(open + 1)], Split(raw[(open + 1)..^1]));
    }

    private static IReadOnlyList<Expr> Split(string body)
    {
        List<Expr> parts = [];
        var literal = new StringBuilder();
        for (int i = 0; i < body.Length; i++)
        {
            if (IsEscapedBrace(body, i))
            {
                literal.Append(body, i++, 2);
                continue;
            }
            int end = body[i] == '{' ? FindHoleEnd(body, i) : -1;
            if (end < 0)
            {
                literal.Append(body[i]);
                continue;
            }
            Flush(parts, literal);
            parts.Add(Hole(body[(i + 1)..end]));
            i = end;
        }
        Flush(parts, literal);
        return parts;
    }

    private static bool IsEscapedBrace(string body, int i)
        => body[i] is '{' or '}' && i + 1 < body.Length && body[i + 1] == body[i];

    private static void Flush(List<Expr> parts, StringBuilder literal)
    {
        if (literal.Length == 0)
            return;

        parts.Add(new Literal(literal.ToString()));
        literal.Clear();
    }

    /// <summary>
    /// The alignment and format specifiers are separated from the expression by a top-level
    /// <c>,</c> or <c>:</c>. C# reserves both inside a hole — a conditional has to be
    /// parenthesised there — so the first one at depth zero always ends the expression.
    /// </summary>
    private static Expr Hole(string hole)
    {
        int split = IndexAtDepthZero(hole, ",:");
        var source = split < 0 ? hole : hole[..split];
        var suffix = split < 0 ? string.Empty : hole[split..];
        return new Interpolation($"{{{hole}}}", Parser.Parse(source.Trim()), suffix);
    }

    private static int FindHoleEnd(string body, int start)
    {
        int end = IndexAtDepthZero(body[(start + 1)..], "}");
        return end < 0 ? -1 : start + 1 + end;
    }

    /// Scans for any of <paramref name="targets"/> outside brackets and outside quoted text.
    private static int IndexAtDepthZero(string text, string targets)
    {
        int depth = 0;
        char quote = default;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (quote != default)
            {
                if (c == quote)
                    quote = default;
                continue;
            }
            if (c is '"' or '\'')
                quote = c;
            else if (depth == 0 && targets.Contains(c))
                return i;
            else if (c is '(' or '[' or '{')
                depth++;
            else if (c is ')' or ']' or '}')
                depth--;
        }
        return -1;
    }
}
