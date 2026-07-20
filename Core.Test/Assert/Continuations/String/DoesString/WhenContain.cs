using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.DoesString;

public class WhenContain : StringSpec
{
    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("xabcyz", "abc")]
    public void GivenContainString_ThenDoesNotThrow(string text, string expected)
        => text.Does().Contain(expected).and.Is().not.Null();

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "abcd")]
    [InlineData("abc", "Abc")]
    public void GivenNotContainString_ThenGetException(string? text, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Does().Contain(expected));
        ex.HasMessage($"Expected text to contain {Describe(expected)} but found {Describe(text)}", "Text contains expected");
    }
}