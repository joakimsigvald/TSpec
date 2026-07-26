using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

public class WhenLocateProjectDirectory : Spec
{
    [Fact]
    public void ThenFindTheNearestAncestorHoldingAProjectFile()
    {
        using var project = new TempProject("MyHotel.Spec");
        ProjectDirectory.Locate(project.BaseDirectory).Is(project.Root);
    }

    [Fact]
    public void GivenTheDirectoryItselfHoldsOne_ThenFindIt()
    {
        using var project = new TempProject("MyHotel.Spec");
        ProjectDirectory.Locate(project.Root).Is(project.Root);
    }
}
