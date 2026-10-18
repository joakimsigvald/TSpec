using TSpec.Architecture;
using TSpec.Assert;
using TSpec.Extensibility;
using TSpec.Test.Internal.Document;

namespace TSpec.Test.Extensibility;

public class WhenReadTheBuild : Spec
{
    [Fact]
    public void GivenAProjectOfTheBuild_ThenOpenItsDebugInformation()
    {
        Build.TryOpenDebugInformation(Source.Projects["."], out var provider).Is().True();
        provider!.Dispose();
    }

    [Fact]
    public void GivenAProjectWithNoAssemblyInTheBuildOutput_ThenOpenNothing()
        => Build.TryOpenDebugInformation(new SourceProjects(new() { ["Nowhere"] = [] })["Nowhere"], out _).Is().False();

    [Fact]
    public void ThenFindTheFolderOfTheProjectTheSourcesBelongTo()
    {
        using var project = new TempProject("Entry");
        Build.TryFindProjectFolder([project.AddSource("Rooms", "RoomEndpoints.cs")], out var folder).Is().True();
        folder.Is(project.Root);
    }

    [Fact]
    public void GivenSourcesThatAreNotOnDisk_ThenFindNoFolder()
        => Build.TryFindProjectFolder(["/_/Entry/RoomEndpoints.cs"], out _).Is().False();
}
