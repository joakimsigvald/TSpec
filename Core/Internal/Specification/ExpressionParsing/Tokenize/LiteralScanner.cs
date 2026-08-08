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
        var quoteStart = start;
        while (quoteStart < input.Length && input[quoteStart] is '$' or '@')
            quoteStart++;

        var quotes = CountLeadingOccurances(input, quoteStart, '"');
        if (quotes == 0)
            return false;

        end = quotes >= 3
            ? EndOfRawBody(input, quoteStart + quotes, quotes)
            : EndOfBody(input, quoteStart + 1, '"', input[start..quoteStart]);
        return true;
    }

    public static bool TryFindCharEnd(string input, int start, out int end)
    {
        end = 0;
        if (start >= input.Length || input[start] != '\'')
            return false;

        end = EndOfBody(input, start + 1, '\'', prefix: "");
        return true;
    }

    internal static int QuoteRun(string input, int from) => CountLeadingOccurances(input, from, '"');

    private static int EndOfBody(string input, int from, char delimiter, string prefix)
    {
        for (var p = from; p < input.Length;)
        {
            if (Escape(input, p, prefix) is var escaped and > 0)
                p += escaped;
            else if (input[p] == delimiter)
                return p + 1;
            else
                p++;
        }
        return input.Length;
    }

    private static int EndOfRawBody(string input, int from, int quotes)
    {
        for (var p = from; p < input.Length;)
        {
            var run = CountLeadingOccurances(input, p, '"');
            if (run >= quotes)
                return p + run;

            p += Math.Max(run, 1);
        }
        return input.Length;
    }

    /// <summary>
    /// How many characters at <paramref name="at"/> belong to the body rather than end it: a
    /// backslash escape where the prefix allows one, a delimiter doubled to escape itself, or a
    /// whole interpolation hole.
    /// </summary>
    private static int Escape(string input, int at, string prefix)
    {
        var verbatim = prefix.Contains('@');
        var interpolated = prefix.Contains('$');
        var selfEscaping = (verbatim ? "\"" : "") + (interpolated ? "{}" : "");
        return input[at] switch
        {
            '\\' when !verbatim && at + 1 < input.Length => 2,
            var ch when selfEscaping.Contains(ch) && CountLeadingOccurances(input, at, ch) >= 2 => 2,
            '{' when interpolated => SizeOfHole(input, at),
            _ => 0,
        };
    }

    private static int SizeOfHole(string input, int at)
    {
        var depth = 0;
        for (var p = at; p < input.Length; p++)
        {
            if (input[p] == '{')
                depth++;
            else if (input[p] == '}' && --depth == 0)
                return p + 1 - at;
        }
        return input.Length - at;
    }

    private static int CountLeadingOccurances(string input, int from, char ch)
        => input.Length - from - input.AsSpan(from).TrimStart(ch).Length;
}
