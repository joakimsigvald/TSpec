using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNotEmpty : StringSpec
{
    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    public void GivenNotEmpty_ThenDoesNotThrow(string? actual)
        => actual.Is().not.Empty().and.not.Empty();

    [Theory]
    [InlineData("")]
    public void GivenEmpty_ThenGetException(string actual)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().not.Empty());
        ex.HasMessage($"Expected actual to not be empty but found {Describe(actual)}", "Actual is not empty");
    }
}