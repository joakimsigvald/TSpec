using TSpec.Architecture;
using TSpec.Architecture.CSharp;

namespace MyHotel.Architecture.Spec;

/// <summary>A project's top-level folders as an invariant over the source: a top-level folder is a
/// sub-domain, and no two of them use each other, directly or around a longer loop. What lies below a
/// top-level folder is its mechanism, free to arrange. A folder uses another when its code uses a type the
/// other declares; a file at the project's root is in no folder. Core also draws its graph
/// (SubdomainDependencies). One fact per project (Sources).</summary>
public class FolderCycles : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void NoTopLevelFoldersUseEachOtherAroundALoop(string project) =>
        Source.Projects[project].Folders.Is().Acyclic();
}
