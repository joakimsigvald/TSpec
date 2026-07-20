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
    public void GivenStartWithString_ThenDoesNotThrow(string? text, string? expected)
        => text.Does().not.StartWith(expected).and.not.StartWith(expected);

    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("xabcyz", "xab")]
    public void GivenNotStartWithString_ThenGetException(string text, string expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Does().not.StartWith(expected));
        ex.HasMessage($"Expected text to not start with {Describe(expected)} but found {Describe(text)}",
            "Text does not start with expected");
    }
}