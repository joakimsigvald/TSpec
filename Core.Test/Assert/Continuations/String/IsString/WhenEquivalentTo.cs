using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenEquivalentTo : StringSpec
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("abc", "ABC")]
    public void GivenEquivalentToString_ThenDoesNotThrow(string? text, string? expected)
        => text.Is().EquivalentTo(expected).and.Does().not.Contain("XXX");

    [Theory]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "abcd")]
    public void GivenNotEquivalentToString_ThenGetException(string? text, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Is().EquivalentTo(expected));
        ex.HasMessage($"Expected text to be equivalent to {Describe(expected)} but found {Describe(text)}",
            "Text is equivalent to expected");
    }
}