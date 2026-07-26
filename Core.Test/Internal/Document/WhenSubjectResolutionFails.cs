using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// The failure message is the whole user experience of a misconfigured project, so it states
/// both halves of the rule whichever half broke.
/// </summary>
public class WhenSubjectResolutionFails : Spec
{
    private static string MessageFor(string specAssemblyName)
        => Xunit.Assert.Throws<SetupFailed>(
            () => SpecificationSubject.Resolve(
                specAssemblyName, ProjectReferences.Parse(DepsJson._myHotelSpec, "MyHotel.Spec")))
            .Message;

    [Theory]
    [InlineData("MyHotelSpec")]
    [InlineData("Nonexistent.Spec")]
    [InlineData("MyHotel.Persistence.Spec")]
    public void ThenExplainHowToNameTheSpecProject(string specAssemblyName)
        => MessageFor(specAssemblyName).Does()
            .Contain("'MyHotel.Spec' is preferred").and
            .Contain("'MyHotel.Test' is fine");

    [Theory]
    [InlineData("MyHotelSpec")]
    [InlineData("Nonexistent.Spec")]
    [InlineData("MyHotel.Persistence.Spec")]
    public void ThenExplainThatTheSubjectMustBeReferencedDirectly(string specAssemblyName)
        => MessageFor(specAssemblyName).Does().Contain("reference that project directly");

    [Fact]
    public void GivenNoSuffix_ThenNameTheAssembly()
        => MessageFor("MyHotelSpec").Does().Contain("'MyHotelSpec'");

    [Fact]
    public void GivenTheSubjectIsNotReferenced_ThenListTheReferencesFound()
        => MessageFor("Nonexistent.Spec").Does()
            .Contain("'Nonexistent'").and
            .Contain("MyHotel");
}
