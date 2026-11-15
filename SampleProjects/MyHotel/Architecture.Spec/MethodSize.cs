using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD.md 4a as an invariant over the source: a member's size - TSpec's measure, its
/// lines weighed by their indentation - is at most the ceiling, and so is the size of a file's top-level
/// statements, measured as one member. A local function's lines count in its method's size too. One fact
/// per project (Sources).</summary>
public class MethodSize : TSpec.Spec
{
    private const int Ceiling = 240;

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AMembersSizeIsLimited(string project) =>
        Source.Projects[project].Members.Where(Oversized).Select(Reported).Is().Empty();

    [Theory, MemberData(nameof(Projects))]
    public void TheTopLevelStatementsSizeIsLimited(string project) =>
        Source.Projects[project].Files.Where(OversizedTopLevel).Select(Reported).Is().Empty();

    private static bool Oversized(DeclaredMember member) => member.Size > Ceiling;
    private static bool OversizedTopLevel(SourceFile file) => file.TopLevelSize > Ceiling;
    private static string Reported(DeclaredMember member) => $"{member} ({member.Size:0})";
    private static string Reported(SourceFile file) => $"{file} ({file.TopLevelSize:0})";
}
