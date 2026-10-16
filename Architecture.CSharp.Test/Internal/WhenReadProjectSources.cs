using TSpec.Architecture.CSharp.Internal;
using TSpec.Assert;
using TSpec.Test.Internal.Document;

namespace TSpec.Architecture.CSharp.Test.Internal;

/// <summary>
/// A project's sources picked from the files its debug information lists, which name the whole
/// compilation: what the build generated and what was linked in from elsewhere too.
/// </summary>
public class WhenReadProjectSources : Spec
{
    [Fact]
    public void ThenListTheFilesUnderItsFolderByTheirPathInOrdinalOrder()
    {
        using var project = new TempProject("Entry");
        ProjectSources.From("Entry", [project.AddSource("RoomEndpoints.cs"), project.AddSource("Bookings", "BookingEndpoints.cs")])
            .Select(file => file.Path).Is().EqualTo(["Bookings/BookingEndpoints.cs", "RoomEndpoints.cs"]);
    }

    [Fact]
    public void ThenReadEachFileWhereTheDebugInformationSaysItIs()
    {
        using var project = new TempProject("Entry");
        ProjectSources.From("Entry", [project.AddSource("Rooms", "RoomEndpoints.cs")]).Single().Raw.Is("// a spec");
    }

    [Fact]
    public void GivenAFileGeneratedUnderObj_ThenLeaveItOut()
    {
        using var project = new TempProject("Entry");
        ProjectSources.From(
            "Entry", [project.AddSource("RoomEndpoints.cs"), project.AddSource("obj", "Debug", "net10.0", "Entry.AssemblyInfo.cs")])
            .Select(file => file.Path).Is().EqualTo(["RoomEndpoints.cs"]);
    }

    [Fact]
    public void GivenAFileLinkedInFromElsewhere_ThenLeaveItOut()
    {
        using var project = new TempProject("Entry");
        using var shared = new TempProject("Shared");
        ProjectSources.From(
            "Entry", [shared.AddSource("Linked.cs"), project.AddSource("RoomEndpoints.cs"), project.AddSource("VersionEndpoint.cs")])
            .Select(file => file.Path).Is().EqualTo(["RoomEndpoints.cs", "VersionEndpoint.cs"]);
    }

    [Fact]
    public void GivenFilesThatAreNotOnDisk_ThenRefuse()
        => Xunit.Assert.Throws<SetupFailed>(() => ProjectSources.From("Entry", ["/_/Entry/RoomEndpoints.cs"]))
            .Message.Is("TSpec could not read the sources of 'Entry': they are not where its debug information says, "
                + "as when a build maps source paths.");
}
