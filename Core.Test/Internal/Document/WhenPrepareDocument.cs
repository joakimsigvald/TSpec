using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// The whole resolution chain — manifest, subject, project directory, rendering — against a
/// real directory. The fixture constructor adds only the lookup of the spec assembly's name.
/// </summary>
public class WhenPrepareDocument : Spec
{
    [Fact]
    public void ThenTargetTheProjectRoot()
    {
        using var project = new TempProject("MyHotel.Spec", DepsJson.MyHotelSpec);
        PendingDocument.Prepare("MyHotel.Spec", "4f2a9c1e", project.BaseDirectory)
            .Path.Is(Path.Combine(project.Root, "SPECIFICATION.md"));
    }

    [Fact]
    public void ThenRenderTheSubjectItResolved()
    {
        using var project = new TempProject("MyHotel.Spec", DepsJson.MyHotelSpec);
        PendingDocument.Prepare("MyHotel.Spec", "4f2a9c1e", project.BaseDirectory)
            .Render([]).Does().Contain("# MyHotel").and.Contain("Version 0.1.0");
    }

    [Fact]
    public void ThenWriteWhatItPrepared()
    {
        using var project = new TempProject("MyHotel.Spec", DepsJson.MyHotelSpec);
        var document = PendingDocument.Prepare("MyHotel.Spec", "4f2a9c1e", project.BaseDirectory);
        document.Write([]);
        File.ReadAllText(document.Path).Is(document.Render([]));
    }

    [Fact]
    public void GivenNoDependencyManifest_ThenFail()
    {
        using var project = new TempProject("MyHotel.Spec");
        var error = Xunit.Assert.Throws<SetupFailed>(
            () => PendingDocument.Prepare("MyHotel.Spec", "4f2a9c1e", project.BaseDirectory));
        error.Message.Does().Contain("MyHotel.Spec.deps.json");
    }

    [Fact]
    public void GivenNoProjectFile_ThenFail()
    {
        using var project = new TempProject("MyHotel.Spec", DepsJson.MyHotelSpec, withProjectFile: false);
        Xunit.Assert.Throws<SetupFailed>(
            () => PendingDocument.Prepare("MyHotel.Spec", "4f2a9c1e", project.BaseDirectory));
    }
}
