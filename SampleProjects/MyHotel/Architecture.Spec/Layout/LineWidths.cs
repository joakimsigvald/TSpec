using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 11a as an invariant over the source: a line of code is at most 110
/// characters. The text of a string over several lines is no code (an embedded document or query has its own
/// shape, 0e).</summary>
public class LineWidths : TSpec.Spec
{
    private const int Limit = 110;

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void ALineIsAtMost110Characters(string project) =>
        Sources.CodeLinesOf(project).Has().None(Overwide);

    private static bool Overwide(SourceLine line) => line.Text.Length > Limit;
}
