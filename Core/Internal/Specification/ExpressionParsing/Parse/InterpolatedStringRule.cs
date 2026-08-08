using System.Text;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;
using TSpec.Internal.Specification.ExpressionParsing.Tokenize;

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
        int quotes = open < 0 ? 0 : LiteralScanner.QuoteRun(raw, open);
        if (quotes == 0 || raw.Length < open + 2 * quotes || !raw.EndsWith(new string('"', quotes)))
            return new InterpolatedString(raw, raw, [new Literal(raw)]);

        // The dollar run says how many braces open a hole; fewer than that stay literal. None of
        // them means no holes at all — a raw string with no dollar is literal throughout.
        int braces = raw[..open].Count(c => c == '$');
        var body = raw[(open + quotes)..^quotes];
        return new InterpolatedString(
            raw, raw[..(open + quotes)], braces == 0 ? [new Literal(body)] : Split(body, braces));
    }

    private static IReadOnlyList<Expr> Split(string body, int braces)
    {
        List<Expr> parts = [];
        var literal = new StringBuilder();
        for (int i = 0; i < body.Length;)
        {
            if (braces == 1 && IsEscapedBrace(body, i))
            {
                literal.Append(body, i, 2);
                i += 2;
                continue;
            }
            int hole = OpensHole(body, i, braces) ? FindHoleEnd(body, i + braces) : -1;
            if (hole < 0)
            {
                literal.Append(body[i++]);
                continue;
            }
            Flush(parts, literal);
            parts.Add(Hole(body[(i + braces)..hole]));
            i = hole + braces;
        }
        Flush(parts, literal);
        return parts;
    }

    private static bool OpensHole(string body, int i, int braces)
        => i + braces <= body.Length && body.AsSpan(i, braces).IndexOfAnyExcept('{') < 0;

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
        int end = IndexAtDepthZero(body[start..], "}");
        return end < 0 ? -1 : start + end;
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
