using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert;

// Deliberately not a Spec: verifies that TSpec.Assert works without a test pipeline
public class WhenStandalone
{
    [Fact]
    public void ThenValueAssertionChainPasses() => 3.Is().GreaterThan(2).and.LessThan(4);

    [Fact]
    public void ThenStringAssertionPasses() => "abc".Does().StartWith("ab").and.Contain("bc");

    [Fact]
    public void ThenEnumerableAssertionPasses() => new[] { 1, 2, 3 }.Has().Count(3).and.All(i => i > 0);

    [Fact]
    public void GivenFailingAssertion_ThenThrowsXunitException()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => 3.Is().GreaterThan(4));
        ex.Message.Is("Expected 3 to be greater than 4 but found 3");
        ex.InnerException!.Message.Does().Contain("3 is greater than 4");
    }

    [Fact]
    public void GivenEitherOr_ThenAssertionPasses() => 3.Is().either.GreaterThan(4).or.LessThan(4);
}
