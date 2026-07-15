using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Pipeline;

public class WhenSpecificationHasOtherLineEndings : Spec<MyStateService, int>
{
    [Fact]
    public void GivenExpectedHasWindowsLineEndings_ThenSpecificationMatches()
    {
        When(_ => ++_.Counter).Then().Result.Is(1);
        Specification.Is("When ++_.Counter\r\nThen Result is 1");
    }

    [Fact]
    public void GivenExpectedHasUnixLineEndings_ThenSpecificationMatches()
    {
        When(_ => ++_.Counter).Then().Result.Is(1);
        Specification.Is("When ++_.Counter\nThen Result is 1");
    }

    [Fact]
    public void GivenExpectedTextDiffers_ThenAssertionStillFails()
    {
        When(_ => ++_.Counter).Then().Result.Is(1);
        Xunit.Assert.Throws<XunitException>(
            () => Specification.Is("When ++_.Counter\nThen Result is 2"));
    }
}
