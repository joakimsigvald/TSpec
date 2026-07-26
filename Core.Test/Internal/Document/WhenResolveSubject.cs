using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

public class WhenResolveSubject : Spec
{
    private static SpecificationSubject Resolve(string specAssemblyName)
        => SpecificationSubject.Resolve(
            specAssemblyName, ProjectReferences.Parse(DepsJson._myHotelSpec, "MyHotel.Spec"));

    [Fact] public void ThenNameTheReferencedProject() => Resolve("MyHotel.Spec").Name.Is("MyHotel");

    [Fact] public void ThenTakeItsVersionFromTheBuild() => Resolve("MyHotel.Spec").Version.Is("0.1.0");

    [Fact]
    public void GivenTheDerivedNameIsNotReferenced_ThenFail()
    {
        var error = Xunit.Assert.Throws<SetupFailed>(() => Resolve("Nonexistent.Spec"));
        Xunit.Assert.Contains("'Nonexistent'", error.Message);
    }

    [Fact]
    public void GivenTheDerivedNameIsOnlyTransitivelyReferenced_ThenFail()
        => Xunit.Assert.Throws<SetupFailed>(() => Resolve("MyHotel.Persistence.Spec"));
}
