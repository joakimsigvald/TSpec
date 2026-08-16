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

    /// <summary>
    /// Everything the subject is built on. A rule can live in any of them, so a document generated
    /// before one of them changed describes code that is no longer there — which the id has to say.
    /// </summary>
    [Fact] public void ThenReachEverythingTheSubjectIsBuiltOn()
        => Closure("MyHotel").Is().EqualTo(["MyHotel", "MyHotel.Persistence"]);

    /// <summary>
    /// A package is versioned, not built here, and has no source in the output to read — so the
    /// closure stops at it rather than walking through it.
    /// </summary>
    [Fact] public void ThenStopAtPackages()
        => Closure("MyHotel.Spec").Does().not.Contain("xunit.v3").and.not.Contain("TSpec");

    /// <summary>
    /// Walked from the subject rather than from the spec project, so the test framework and any
    /// project only the specs reference stay out of an id that names the code under test.
    /// </summary>
    [Fact] public void ThenStartWhereItIsAsked()
        => Closure("MyHotel.Persistence").Is().EqualTo(["MyHotel.Persistence"]);

    [Fact] public void GivenAnUnknownSubject_ThenReachNothing()
        => Closure("Other").Is().Empty();

    private static string[] Closure(string name) => [.. Parse().ClosureFrom(name).Order(StringComparer.Ordinal)];
}
