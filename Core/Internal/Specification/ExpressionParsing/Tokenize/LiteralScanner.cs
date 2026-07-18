namespace TSpec.Internal.Specification.ExpressionParsing.Tokenize;

/// <summary>
/// Shared C#-literal boundary detection. Tokenizer uses it to slice
/// string/char tokens; <c>SourcePreprocessor</c> uses it to skip past
/// literals when stripping <c>//</c> line comments.
/// </summary>
internal static class LiteralScanner
{
    public static bool TryFindStringEnd(string input, int start, out int end)
    {
        end = 0;
        var (contentStart, verbatim, interpolated) = ReadStringOpen(input, start);
        if (contentStart < 0)
            return false;

        end = SkipStringContent(input, contentStart, verbatim, interpolated);
        return true;
    }

    public static bool TryFindCharEnd(string input, int start, out int end)
    {
        end = 0;
        if (start >= input.Length || input[start] != '\'')
            return false;

        int p = start + 1;
        while (p < input.Length && input[p] != '\'')
        {
            if (input[p] == '\\' && p + 1 < input.Length)
                p++;
            p++;
        }
        end = p < input.Length ? p + 1 : p;
        return true;
    }

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

    /// Scans the string body to the closing <c>"</c>, respecting <c>\</c>
    /// escapes (non-verbatim), <c>""</c> escapes (verbatim), and balanced
    /// <c>{ }</c> interpolation holes.
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
            if (ch == '"')
                return p + 1;

            p++;
        }
        return p;
    }

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
