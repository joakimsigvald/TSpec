using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

public class WhenParseProjectReferences : Spec
{
    private static ProjectReferences Parse() => ProjectReferences.Parse(DepsJson.MyHotelSpec, "MyHotel.Spec");

    [Fact] public void ThenKeepDirectProjectReferences() => Parse().TryGetVersion("MyHotel", out _).Is(true);

    [Fact] public void ThenReadTheirVersion()
    {
        Parse().TryGetVersion("MyHotel", out var version);
        version.Is("0.1.0");
    }

    [Fact] public void ThenDropPackageReferences() => Parse().TryGetVersion("xunit.v3", out _).Is(false);

    [Fact] public void ThenDropTransitiveProjectReferences()
        => Parse().TryGetVersion("MyHotel.Persistence", out _).Is(false);

    [Fact] public void GivenAnUnknownAssembly_ThenFindNothing()
        => ProjectReferences.Parse(DepsJson.MyHotelSpec, "Other").Names.Count.Is(0);
}
