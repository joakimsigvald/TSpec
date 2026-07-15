namespace TSpec.Internal.Specification.ExpressionParsing.Tokenize;

/// <summary>
/// Shared C#-literal boundary detection. Tokenizer uses it to slice
/// string/char tokens; <c>ExpressionParser</c> uses it to skip past literals
/// when stripping <c>//</c> line comments. Either way the goal is the same —
/// given a position, find where the literal ends.
/// </summary>
internal static class LiteralScanner
{
    /// If a string literal opens at <paramref name="start"/> (optionally
    /// prefixed by <c>$</c>/<c>@</c>), returns the position past its closing
    /// quote. Returns -1 if there is no string literal here.
    public static int TryFindStringEnd(string input, int start)
    {
        var (ContentStart, Verbatim, Interpolated) = ReadStringOpen(input, start);
        if (ContentStart < 0) return -1;
        return SkipStringContent(input, ContentStart, Verbatim, Interpolated);
    }

    /// If a char literal opens at <paramref name="start"/>, returns the
    /// position past its closing apostrophe. Returns -1 otherwise.
    public static int TryFindCharEnd(string input, int start)
    {
        if (start >= input.Length || input[start] != '\'') return -1;
        int p = start + 1;
        while (p < input.Length && input[p] != '\'')
        {
            if (input[p] == '\\' && p + 1 < input.Length) p++;
            p++;
        }
        return p < input.Length ? p + 1 : p;
    }

    /// Reads any <c>$</c>/<c>@</c> prefix followed by the opening <c>"</c>.
    /// Returns the position just after the opening quote, or <c>ContentStart = -1</c>
    /// if no string opens here.
    private static (int ContentStart, bool Verbatim, bool Interpolated) ReadStringOpen(string input, int start)
    {
        int p = start;
        bool verbatim = false, interpolated = false;
        while (p < input.Length && input[p] is '$' or '@')
        {
            if (input[p] == '$') interpolated = true; else verbatim = true;
            p++;
        }
        return p < input.Length && input[p] == '"' ? (p + 1, verbatim, interpolated) : (-1, false, false);
    }

    /// Scans the string body from <paramref name="from"/> to the closing
    /// <c>"</c>, respecting <c>\</c> escapes (non-verbatim), <c>""</c> escapes
    /// (verbatim), and balanced <c>{ }</c> interpolation holes.
    private static int SkipStringContent(string input, int from, bool verbatim, bool interpolated)
    {
        int p = from;
        while (p < input.Length)
        {
            char ch = input[p];
            if (interpolated && ch == '{')
            {
                if (IsDoubled(input, p, '{')) { p += 2; continue; }
                p = SkipInterpolationHole(input, p + 1);
                continue;
            }
            if (interpolated && ch == '}' && IsDoubled(input, p, '}')) { p += 2; continue; }
            if (!verbatim && ch == '\\' && p + 1 < input.Length) { p += 2; continue; }
            if (verbatim && ch == '"' && IsDoubled(input, p, '"')) { p += 2; continue; }
            if (ch == '"') return p + 1;
            p++;
        }
        return p;
    }

    /// Walks past the matching <c>}</c> for a <c>{</c> we've already consumed.
    /// Tracks brace depth so nested holes don't fool the scanner.
    private static int SkipInterpolationHole(string input, int from)
    {
        int p = from, depth = 1;
        while (p < input.Length && depth > 0)
        {
            if (input[p] == '{') depth++;
            else if (input[p] == '}') depth--;
            p++;
        }
        return p;
    }

    private static bool IsDoubled(string input, int p, char ch)
        => p + 1 < input.Length && input[p + 1] == ch;
}
