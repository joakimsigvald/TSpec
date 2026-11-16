using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Sizes;

/// <summary>CODING-STANDARD-BACKEND.md 10d as an invariant over the source: no file is over 250 lines - one
/// that grows past it is split.</summary>
public class FileLengths : TSpec.Spec
{
    private const int Limit = 250;

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AFileIsUnder250Lines(string project) =>
        Source.Projects[project].Files.Has().None(file => Overlong(file));

    private static bool Overlong(SourceFile file) => file.Lines.Count > Limit;
}
