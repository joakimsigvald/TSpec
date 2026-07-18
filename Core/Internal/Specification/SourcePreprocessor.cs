using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using TSpec.Internal.Specification.ExpressionParsing.Tokenize;

namespace TSpec.Internal.Specification;

/// <summary>
/// Normalizes a compiler-provided expression string before parsing:
/// strips <c>//</c> line comments (respecting string/char literals) and merges
/// multi-line expressions into a single line.
/// </summary>
internal static partial class SourcePreprocessor
{
    private static readonly char[] _noSpaceAfterCues = ['.', '(', '['];

    [return: NotNullIfNotNull(nameof(str))]
    public static string? ToSingleLine(this string? str)
        => string.IsNullOrEmpty(str) ? str : MergeLines([.. ToLines(str)]);

    private static IEnumerable<string> ToLines(string str)
        => LineBreakRegex()
        .Split(str)
        .Select(StripLineComment)
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Select(s => s.Trim());

    private static string MergeLines(this string[] lines)
    {
        StringBuilder sb = new();
        bool addSpace = false;
        foreach (var line in lines)
        {
            if (addSpace && !line.StartsWith('.'))
                sb.Append(' ');
            sb.Append(line);
            addSpace = !_noSpaceAfterCues.Contains(line[^1]);
        }
        return sb.ToString();
    }

    private static string StripLineComment(string line)
    {
        for (var i = 0; i < line.Length; i = Advance(line, i))
            if (line[i..].StartsWith("//"))
                return line[..i];
        return line;
    }

    private static int Advance(string line, int i)
    {
        if (LiteralScanner.TryFindCharEnd(line, i, out int charEnd))
            return charEnd;

        if (LiteralScanner.TryFindStringEnd(line, i, out int strEnd))
            return strEnd;

        return i + 1;
    }

    [GeneratedRegex(@"(\r|\n)+")]
    private static partial Regex LineBreakRegex();
}
