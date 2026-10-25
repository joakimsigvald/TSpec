namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The indentation level of each line of a file: the rank of its width among the widths the file's lines of
/// code start at, so a line indented between two others makes a level of its own.
/// </summary>
internal static class LineLevels
{
    private const int DefaultTabWidth = 4;

    // A line of text indents by the text's own steps, so it stands one level in from the line its text opens on.
    internal static IReadOnlyList<int> Of(IReadOnlyList<string> lines, IReadOnlyDictionary<int, int> openings)
    {
        var code = CodeLevels(lines, openings);
        return [.. code.Select((level, index) => openings.TryGetValue(index, out var opening) ? code[opening] + 1 : level)];
    }

    // A blank line has no width of its own and stands at level 0.
    private static IReadOnlyList<int> CodeLevels(IReadOnlyList<string> lines, IReadOnlyDictionary<int, int> openings)
    {
        var levels = LevelsOf(lines.Where((line, index) => IsCode(line, index, openings)).Select(WidthOf));
        return [.. lines.Select((line, index) => IsCode(line, index, openings) ? levels[WidthOf(line)] : 0)];
    }

    private static Dictionary<int, int> LevelsOf(IEnumerable<int> widths)
        => widths.Distinct().Order().Select((width, level) => (width, level))
            .ToDictionary(ranked => ranked.width, ranked => ranked.level);

    private static bool IsCode(string line, int index, IReadOnlyDictionary<int, int> openings)
        => !string.IsNullOrWhiteSpace(line) && !openings.ContainsKey(index);

    private static int WidthOf(string line) => line.TakeWhile(IsIndent).Aggregate(0, Advance);

    private static bool IsIndent(char character) => character is ' ' or '\t';

    private static int Advance(int width, char indent) => indent == '\t' ? width + DefaultTabWidth - width % DefaultTabWidth : width + 1;
}
