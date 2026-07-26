using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

public class WhenDeriveSubjectName : Spec
{
    [Theory]
    [InlineData("MyHotel.Spec", "MyHotel")]
    [InlineData("MyHotel.Test", "MyHotel")]
    [InlineData("MyHotel.Whatever", "MyHotel")]
    [InlineData("MyHotel.Api.Spec", "MyHotel.Api")]
    public void GivenASuffix_ThenStripTheLastOne(string specAssemblyName, string expected)
        => SpecificationSubject.DeriveName(specAssemblyName).Is(expected);

    [Theory]
    [InlineData("MyHotelSpec")]
    [InlineData(".Spec")]
    [InlineData("MyHotel.")]
    public void GivenNothingToStrip_ThenFail(string specAssemblyName)
        => Xunit.Assert.Throws<SetupFailed>(() => SpecificationSubject.DeriveName(specAssemblyName));
}
