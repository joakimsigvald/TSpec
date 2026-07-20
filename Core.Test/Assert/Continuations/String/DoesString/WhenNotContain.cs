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
    public void GivenNotContainString_ThenDoesNotThrow(string? text, string? expected)
        => text.Does().not.Contain(expected).and.not.Contain(expected);

    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("xabcyz", "abc")]
    public void GivenContainString_ThenGetException(string text, string expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Does().not.Contain(expected));
        ex.HasMessage($"Expected text to not contain {Describe(expected)} but found {Describe(text)}", "Text does not contain expected");
    }
}