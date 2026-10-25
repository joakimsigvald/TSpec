namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The indentation level of each line of a file: the rank of its width among the widths the file's lines
/// start at, so a line indented between two others makes a level of its own.
/// </summary>
internal static class LineLevels
{
    private const int DefaultTabWidth = 4;

    // A blank line has no width of its own and stands at level 0.
    internal static IReadOnlyList<int> Of(IReadOnlyList<string> lines)
    {
        var levels = LevelsOf(lines.Where(IsWritten).Select(WidthOf));
        return [.. lines.Select(line => IsWritten(line) ? levels[WidthOf(line)] : 0)];
    }

    private static Dictionary<int, int> LevelsOf(IEnumerable<int> widths)
        => widths.Distinct().Order().Select((width, level) => (width, level))
            .ToDictionary(ranked => ranked.width, ranked => ranked.level);

    private static bool IsWritten(string line) => !string.IsNullOrWhiteSpace(line);

    private static int WidthOf(string line) => line.TakeWhile(IsIndent).Aggregate(0, Advance);

    private static bool IsIndent(char character) => character is ' ' or '\t';

    private static int Advance(int width, char indent) => indent == '\t' ? width + DefaultTabWidth - width % DefaultTabWidth : width + 1;
}
