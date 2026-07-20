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
    public void GivenNotEndWithString_ThenDoesNotThrow(string? text, string? expected)
        => text.Does().not.EndWith(expected).and.not.EndWith(expected);

    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("xabcyz", "cyz")]
    public void GivenEndWithString_ThenGetException(string text, string expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Does().not.EndWith(expected));
        ex.HasMessage($"Expected text to not end with {Describe(expected)} but found {Describe(text)}",
            "Text does not end with expected");
    }
}