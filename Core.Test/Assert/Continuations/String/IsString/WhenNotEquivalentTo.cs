using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNotEquivalentTo : StringSpec
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "abcd")]
    public void GivenNotEquivalentToString_ThenDoesNotThrow(string? actual, string? expected)
        => actual.Is().not.EquivalentTo(expected).and.not.EquivalentTo(expected);

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("abc", "ABC")]
    public void GivenEquivalentToString_ThenGetException(string? actual, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().not.EquivalentTo(expected));
        ex.HasMessage($"Expected actual to not be equivalent to {Describe(expected)} but found {Describe(actual)}",
            "Actual is not equivalent to expected");
    }
}