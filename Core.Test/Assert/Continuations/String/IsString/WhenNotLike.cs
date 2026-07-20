using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNotLike : StringSpec
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "abcd")]
    public void GivenNotLikeString_ThenDoesNotThrow(string? text, string? expected)
        => text.Is().not.Like(expected).and.Does().not.Contain("XXX");

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("abc", "ABC")]
    public void GivenLikeString_ThenGetException(string? text, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Is().not.Like(expected));
        ex.HasMessage($"Expected text to not be like {Describe(expected)} but found {Describe(text)}",
            "Text is not like expected");
    }
}