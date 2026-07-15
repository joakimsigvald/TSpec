using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenEquivalentTo : StringSpec
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("abc", "ABC")]
    public void GivenEquivalentToString_ThenDoesNotThrow(string? actual, string? expected)
        => actual.Is().EquivalentTo(expected).and.Does().not.Contain("XXX");

    [Theory]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "abc")]
    [InlineData("abc", "abcd")]
    public void GivenNotEquivalentToString_ThenGetException(string? actual, string? expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().EquivalentTo(expected));
        ex.HasMessage($"Expected actual to be equivalent to {Describe(expected)} but found {Describe(actual)}",
            "Actual is equivalent to expected");
    }
}