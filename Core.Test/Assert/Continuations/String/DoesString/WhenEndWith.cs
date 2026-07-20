using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.DoesString;

public class WhenEndWith : StringSpec
{
    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("xabcyz", "cyz")]
    public void GivenEndWithString_ThenDoesNotThrow(string text, string expected) 
        => text.Does().EndWith(expected).and.EndWith(expected);

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "ab")]
    [InlineData("abc", "Bc")]
    public void GivenNotEndWithString_ThenGetException(string? text, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Does().EndWith(expected));
        ex.HasMessage($"Expected text to end with {Describe(expected)} but found {Describe(text)}", "Text ends with expected");
    }
}