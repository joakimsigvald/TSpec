using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 11c as an invariant over the source: four spaces per level, so a line
/// is indented in whole levels and never with a tab, and no line indented past the fifth level - the content
/// of a multi-line raw string literal aside (0e). One fact per project (Sources).</summary>
public class NestingDepth : TSpec.Spec
{
    private const int Limit = 5;

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void NoLineIsIndentedPastTheFifthLevel(string project) =>
        Source.Projects[project].Files.SelectMany(OvernestedLines).Is().Empty();

    [Theory, MemberData(nameof(Projects))]
    public void NoLineIsIndentedWithATab(string project) =>
        Source.Projects[project].Files.SelectMany(TabbedLines).Is().Empty();

    [Theory, MemberData(nameof(Projects))]
    public void ALineIsIndentedInWholeLevels(string project) =>
        Source.Projects[project].Files.SelectMany(PartLevelLines).Is().Empty();

    private static IEnumerable<string> OvernestedLines(SourceFile file) =>
        OwnLines(file)
            .Where(line => Indents.LevelOf(line.Text) > Limit)
            .Select(line => $"{file}:{line.Index + 1}");

    private static IEnumerable<string> TabbedLines(SourceFile file) =>
        OwnLines(file)
            .Where(line => line.Text.TrimStart(' ').StartsWith('\t'))
            .Select(line => $"{file}:{line.Index + 1}");

    private static IEnumerable<string> PartLevelLines(SourceFile file) =>
        OwnLines(file)
            .Where(line => IsPartLevel(line.Text))
            .Select(line => $"{file}:{line.Index + 1}");

    /// <summary>A line that starts part-way into a level; a blank line starts nowhere.</summary>
    private static bool IsPartLevel(string line) =>
        line.TrimStart().Length > 0 && Indents.Of(line) % Indents.Width != 0;

    private static IEnumerable<(string Text, int Index)> OwnLines(SourceFile file)
    {
        var embedded = EmbeddedLines.Of(file);
        return file.Lines
            .Select((text, index) => (Text: text, Index: index))
            .Where(line => !embedded.Contains(line.Index));
    }
}
