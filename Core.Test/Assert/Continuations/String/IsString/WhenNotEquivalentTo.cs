using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNotEquivalentTo : StringSpec
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "abcd")]
    public void GivenNotEquivalentToString_ThenDoesNotThrow(string? text, string? expected)
        => text.Is().not.EquivalentTo(expected).and.not.EquivalentTo(expected);

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("abc", "ABC")]
    public void GivenEquivalentToString_ThenGetException(string? text, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Is().not.EquivalentTo(expected));
        ex.HasMessage($"Expected text to not be equivalent to {Describe(expected)} but found {Describe(text)}",
            "Text is not equivalent to expected");
    }
}