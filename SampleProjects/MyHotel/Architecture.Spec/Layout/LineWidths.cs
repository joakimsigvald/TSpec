using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 11a as an invariant over the source: a line is at most 110 characters,
/// the content of a multi-line raw string literal aside (an embedded document or query has its own shape,
/// 0e).</summary>
public class LineWidths : TSpec.Spec
{
    private const int Limit = 110;

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void ALineIsAtMost110Characters(string project) =>
        Source.Projects[project].Files.SelectMany(OverwideLines).Is().Empty();

    /// <summary>"file:line" for every line over the limit.</summary>
    private static IEnumerable<string> OverwideLines(SourceFile file)
    {
        var embedded = EmbeddedLines.Of(file);
        return file.Lines
            .Select((text, index) => (Text: text, Index: index))
            .Where(line => !embedded.Contains(line.Index) && line.Text.Length > Limit)
            .Select(line => $"{file}:{line.Index + 1}");
    }
}
