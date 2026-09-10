using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// Where the specification is written. The source files the spec classes are written in say which
/// project that is; the build output only says so where the build happened to leave it in the tree.
/// </summary>
public class WhenLocateProjectDirectory : Spec
{
    private static string? Locate(IReadOnlyCollection<string> specSources, string baseDirectory)
        => ProjectDirectory.TryLocate(specSources, baseDirectory, out var directory) ? directory : null;

    [Fact]
    public void ThenFindTheProjectTheSpecSourcesAreWrittenIn()
    {
        using var project = new TempProject("MyHotel.Spec");
        Locate([project.AddSource("Rooms", "WhenAddRoom.cs")], project.BaseDirectory).Is(project.Root);
    }

    /// <summary>
    /// The regression this was written for: a build given an artifacts path puts the binaries
    /// outside the tree entirely, where no project file sits above them.
    /// </summary>
    [Fact]
    public void GivenTheOutputIsOutsideTheSourceTree_ThenFindItAnyway()
    {
        using var project = new TempProject("MyHotel.Spec");
        using var artifacts = new TempProject("MyHotel.Spec", withProjectFile: false);
        Locate([project.AddSource("Rooms", "WhenAddRoom.cs")], artifacts.BaseDirectory).Is(project.Root);
    }

    /// <summary>
    /// The quieter half of the same fault: an artifacts folder nested under an unrelated project
    /// used to be taken for the spec project, and the specification was written into it.
    /// </summary>
    [Fact]
    public void GivenTheOutputIsUnderAnotherProject_ThenPreferTheOneTheSourcesAreIn()
    {
        using var project = new TempProject("MyHotel.Spec");
        using var unrelated = new TempProject("Unrelated");
        Locate([project.AddSource("Rooms", "WhenAddRoom.cs")], unrelated.BaseDirectory).Is(project.Root);
    }

    /// A linked file, or one from a shared project, is outvoted rather than followed.
    [Fact]
    public void GivenSourcesInSeveralProjects_ThenTakeTheOneMostOfThemAreIn()
    {
        using var project = new TempProject("MyHotel.Spec");
        using var shared = new TempProject("Shared");
        Locate(
            [shared.AddSource("Linked.cs"), project.AddSource("WhenAddRoom.cs"), project.AddSource("WhenGetRoom.cs")],
            project.BaseDirectory)
            .Is(project.Root);
    }

    /// <summary>
    /// A build that maps source paths records a file that is nowhere on disk, so there is nothing
    /// to walk up from and the output directory answers instead.
    /// </summary>
    [Fact]
    public void GivenASourcePathThatIsNotOnDisk_ThenFallBackToTheOutput()
    {
        using var project = new TempProject("MyHotel.Spec");
        Locate(["/_/Rooms/WhenAddRoom.cs"], project.BaseDirectory).Is(project.Root);
    }

    [Fact]
    public void GivenNoSources_ThenFindTheNearestAncestorOfTheOutputHoldingAProjectFile()
    {
        using var project = new TempProject("MyHotel.Spec");
        Locate([], project.BaseDirectory).Is(project.Root);
    }

    [Fact]
    public void GivenTheOutputDirectoryItselfHoldsOne_ThenFindIt()
    {
        using var project = new TempProject("MyHotel.Spec");
        Locate([], project.Root).Is(project.Root);
    }

    [Fact]
    public void GivenNeitherSourcesNorAProjectFile_ThenLocateNothing()
    {
        using var artifacts = new TempProject("MyHotel.Spec", withProjectFile: false);
        Locate(["/_/Rooms/WhenAddRoom.cs"], artifacts.BaseDirectory).Is().Null();
    }

    /// <summary>
    /// The whole read, against this assembly: its spec classes, their files as the debug
    /// information records them, and the project they are written in — with an output directory
    /// that could not answer.
    /// </summary>
    [Fact]
    public void ThenReadTheSourcesOfARealAssemblyFromItsDebugInformation()
    {
        using var artifacts = new TempProject("TSpec.Test", withProjectFile: false);
        Locate(SpecClasses.SourcesOf(typeof(WhenLocateProjectDirectory).Assembly), artifacts.BaseDirectory)
            .Is(ThisProject());
    }

    /// This file is Core.Test/Internal/Document/WhenLocateProjectDirectory.cs.
    private static string ThisProject([System.Runtime.CompilerServices.CallerFilePath] string here = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..")).TrimEnd(Path.DirectorySeparatorChar);
}
