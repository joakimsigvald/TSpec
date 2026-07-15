using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.DoesString;

public class WhenStartWith : StringSpec
{
    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("xabcyz", "xab")]
    public void GivenStartWithString_ThenDoesNotThrow(string actual, string expected) 
        => actual.Does().StartWith(expected).and.StartWith(expected);

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "bc")]
    [InlineData("abc", "Ab")]
    public void GivenNotStartWithString_ThenGetException(string? actual, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Does().StartWith(expected));
        ex.HasMessage($"Expected actual to start with {Describe(expected)} but found {Describe(actual)}",
            "Actual starts with expected");
    }
}