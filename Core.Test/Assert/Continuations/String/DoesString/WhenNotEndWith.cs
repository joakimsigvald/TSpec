using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.DoesString;

public class WhenNotEndWith : StringSpec
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "ab")]
    [InlineData("abc", "Bc")]
    public void GivenNotEndWithString_ThenDoesNotThrow(string? actual, string? expected)
        => actual.Does().not.EndWith(expected).and.not.EndWith(expected);

    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("xabcyz", "cyz")]
    public void GivenEndWithString_ThenGetException(string actual, string expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Does().not.EndWith(expected));
        ex.HasMessage($"Expected actual to not end with {Describe(expected)} but found {Describe(actual)}",
            "Actual does not end with expected");
    }
}