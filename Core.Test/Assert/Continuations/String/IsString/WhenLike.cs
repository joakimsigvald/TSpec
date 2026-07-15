using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenLike : StringSpec
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("abc", "ABC")]
    public void GivenLikeString_ThenDoesNotThrow(string? actual, string? expected)
        => actual.Is().Like(expected).and.Does().not.Contain("XXX");

    [Theory]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "abcd")]
    public void GivenNotLikeString_ThenGetException(string? actual, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().Like(expected));
        ex.HasMessage($"Expected actual to be like {Describe(expected)} but found {Describe(actual)}",
            "Actual is like expected");
    }
}