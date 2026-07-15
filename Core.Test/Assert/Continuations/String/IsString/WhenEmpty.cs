using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenEmpty : StringSpec
{
    [Fact]
    public void GivenEmpty_ThenDoesNotThrow()
        => "".Is().Empty().and.Does().Contain("");

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    public void GivenNotEmpty_ThenGetException(string? actual)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().Empty());
        ex.HasMessage($"Expected actual to be empty but found {Describe(actual)}", "Actual is empty");
    }
}