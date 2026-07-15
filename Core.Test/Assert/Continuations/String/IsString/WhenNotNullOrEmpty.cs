using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNotNullOrEmpty : StringSpec
{
    [Fact]
    public void GivenNotNullOrEmpty_ThenDoesNotThrow()
        => "abc".Is().not.NullOrEmpty().and.Does().Contain("a");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GivenNullOrEmpty_ThenGetException(string? actual)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().not.NullOrEmpty());
        ex.HasMessage($"Expected actual to not be null or empty but found {Describe(actual)}",
            "Actual is not null or empty");
    }
}