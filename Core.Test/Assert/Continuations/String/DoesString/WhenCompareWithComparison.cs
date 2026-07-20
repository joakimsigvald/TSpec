using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.DoesString;

public class WhenCompareWithComparison : StringSpec
{
    [Theory]
    [InlineData("xABCy", "abc")]
    [InlineData("abc", "ABC")]
    public void GivenContainIgnoringCase_ThenDoesNotThrow(string text, string expected)
    {
        text.Does().Contain(expected, StringComparison.OrdinalIgnoreCase);
        Specification.Is("Text contains expected ignoring case");
    }

    [Theory]
    [InlineData("xyz", "abc")]
    public void GivenNotContainIgnoringCase_ThenGetException(string text, string expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => text.Does().Contain(expected, StringComparison.OrdinalIgnoreCase));
        ex.HasMessage($"Expected text to contain {Describe(expected)} ignoring case but found {Describe(text)}");
    }

    [Theory]
    [InlineData("ABCxyz", "abc")]
    public void GivenStartWithIgnoringCase_ThenDoesNotThrow(string text, string expected)
        => text.Does().StartWith(expected, StringComparison.OrdinalIgnoreCase);

    [Theory]
    [InlineData("xyzABC", "abc")]
    public void GivenEndWithIgnoringCase_ThenDoesNotThrow(string text, string expected)
        => text.Does().EndWith(expected, StringComparison.OrdinalIgnoreCase);

    [Fact]
    public void GivenInvariantCultureComparison_ThenDoesNotThrow()
    {
        var text = "xabcy";
        text.Does().Contain("abc", StringComparison.InvariantCulture);
        Specification.Is("Text contains \"abc\" using invariant culture");
    }
}
