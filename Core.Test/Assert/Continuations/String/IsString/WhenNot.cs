using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNot : StringSpec
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "abcd")]
    [InlineData("abc", "ABC")]
    public void GivenNotSameString_ThenDoesNotThrow(string? text, string? expected)
        => text.Is().Not(expected).and.Does().not.Contain("XXX");

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    public void GivenSameString_ThenGetException(string? text, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Is().Not(expected));
        ex.HasMessage($"Expected text to be not {Describe(expected)} but found {Describe(text)}", "Text is not expected");
    }
}