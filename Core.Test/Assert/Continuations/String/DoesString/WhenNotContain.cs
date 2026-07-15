using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.DoesString;

public class WhenNotContain : StringSpec
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(null, "")]
    [InlineData("", "abc")]
    [InlineData("abc", "abcd")]
    [InlineData("Abc", "abc")]
    public void GivenNotContainString_ThenDoesNotThrow(string? actual, string? expected)
        => actual.Does().not.Contain(expected).and.not.Contain(expected);

    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("xabcyz", "abc")]
    public void GivenContainString_ThenGetException(string actual, string expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Does().not.Contain(expected));
        ex.HasMessage($"Expected actual to not contain {Describe(expected)} but found {Describe(actual)}", "Actual does not contain expected");
    }
}