using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.DoesString;

public class WhenNotStartWith : StringSpec
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "bc")]
    [InlineData("abc", "Ab")]
    public void GivenStartWithString_ThenDoesNotThrow(string? actual, string? expected)
        => actual.Does().not.StartWith(expected).and.not.StartWith(expected);

    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("xabcyz", "xab")]
    public void GivenNotStartWithString_ThenGetException(string actual, string expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Does().not.StartWith(expected));
        ex.HasMessage($"Expected actual to not start with {Describe(expected)} but found {Describe(actual)}",
            "Actual does not start with expected");
    }
}