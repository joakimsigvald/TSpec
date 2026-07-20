using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.DoesString;

public class WhenStartWith : StringSpec
{
    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("xabcyz", "xab")]
    public void GivenStartWithString_ThenDoesNotThrow(string text, string expected) 
        => text.Does().StartWith(expected).and.StartWith(expected);

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "bc")]
    [InlineData("abc", "Ab")]
    public void GivenNotStartWithString_ThenGetException(string? text, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Does().StartWith(expected));
        ex.HasMessage($"Expected text to start with {Describe(expected)} but found {Describe(text)}",
            "Text starts with expected");
    }
}