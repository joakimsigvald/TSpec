using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 11c as an invariant over the source: four spaces per level, so a line
/// of code is indented in whole levels and never with a tab, and no line indented past the fifth level. The
/// text of a string over several lines is no code (0e).</summary>
public class NestingDepth : TSpec.Spec
{
    private const int Limit = 5;

    [Fact]
    public void NoLineIsIndentedPastTheFifthLevel() =>
        Source.Projects.Has().All(project => project.CodeLines.Has().None(Overnested));

    [Fact]
    public void NoLineIsIndentedWithATab() =>
        Source.Projects.Has().All(project => project.CodeLines.Has().None(Tabbed));

    [Fact]
    public void ALineIsIndentedInWholeLevels() =>
        Source.Projects.Has().All(project => project.CodeLines.Has().None(PartLevel));

    private static bool Overnested(SourceLine line) => Indents.LevelOf(line.Text) > Limit;
    private static bool Tabbed(SourceLine line) => line.Text.TrimStart(' ').StartsWith('\t');

    /// <summary>A line that starts part-way into a level; a blank line starts nowhere.</summary>
    private static bool PartLevel(SourceLine line) =>
        line.Text.TrimStart().Length > 0 && Indents.Of(line.Text) % Indents.Width != 0;
}
