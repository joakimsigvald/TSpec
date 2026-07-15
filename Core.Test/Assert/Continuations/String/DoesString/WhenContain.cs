using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.DoesString;

public class WhenContain : StringSpec
{
    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("xabcyz", "abc")]
    public void GivenContainString_ThenDoesNotThrow(string actual, string expected)
        => actual.Does().Contain(expected).and.Is().not.Null();

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "abcd")]
    [InlineData("abc", "Abc")]
    public void GivenNotContainString_ThenGetException(string? actual, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Does().Contain(expected));
        ex.HasMessage($"Expected actual to contain {Describe(expected)} but found {Describe(actual)}", "Actual contains expected");
    }
}