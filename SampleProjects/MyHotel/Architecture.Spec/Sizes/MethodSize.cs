using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Sizes;

/// <summary>CODING-STANDARD-BACKEND.md 4a as an invariant over the source: a member's size - TSpec's measure,
/// its lines weighed by their indentation - is at most the ceiling, and so is the size of a file's top-level
/// statements, measured as one member. A local function's lines count in its method's size too.</summary>
public class MethodSize : TSpec.Spec
{
    private const int Ceiling = 240;

    [Fact]
    public void AMembersSizeIsLimited() =>
        Source.Projects.Has().All(project => project.Members.Where(Oversized).Select(Reported).Is().Empty());

    [Fact]
    public void TheTopLevelStatementsSizeIsLimited() =>
        Source.Projects.Has().All(
            project => project.Files.Where(OversizedTopLevel).Select(Reported).Is().Empty());

    private static bool Oversized(DeclaredMember member) => member.Size > Ceiling;
    private static bool OversizedTopLevel(SourceFile file) => file.TopLevelSize > Ceiling;
    private static string Reported(DeclaredMember member) => $"{member} ({member.Size:0})";
    private static string Reported(SourceFile file) => $"{file} ({file.TopLevelSize:0})";
}
