using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenLike : StringSpec
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("abc", "ABC")]
    public void GivenLikeString_ThenDoesNotThrow(string? text, string? expected)
        => text.Is().Like(expected).and.Does().not.Contain("XXX");

    [Theory]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "abcd")]
    public void GivenNotLikeString_ThenGetException(string? text, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Is().Like(expected));
        ex.HasMessage($"Expected text to be like {Describe(expected)} but found {Describe(text)}",
            "Text is like expected");
    }
}