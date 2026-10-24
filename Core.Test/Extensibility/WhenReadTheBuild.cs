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

    [Fact]
    public void GivenTheAssemblyOfAPackage_ThenFindThePackageAsTheRulesNameIt()
    {
        Build.TryFindPackage("MongoDB.Driver", out var package).Is().True();
        package.Is("P:MongoDB.Driver");
    }

    [Fact]
    public void GivenTheAssemblyOfAPackageOnlyReachedThroughAnother_ThenFindItToo()
    {
        Build.TryFindPackage("MongoDB.Bson", out var package).Is().True();
        package.Is("P:MongoDB.Bson");
    }

    [Fact]
    public void GivenTheAssemblyOfAProject_ThenFindNoPackage()
        => Build.TryFindPackage("TSpec", out _).Is().False();

    [Fact]
    public void GivenAnAssemblyOfTheFramework_ThenFindNoPackage()
        => Build.TryFindPackage("System.Runtime", out _).Is().False();
}
