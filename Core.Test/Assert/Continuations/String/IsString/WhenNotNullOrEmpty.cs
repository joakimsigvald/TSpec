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
    public void GivenNullOrEmpty_ThenGetException(string? text)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Is().not.NullOrEmpty());
        ex.HasMessage($"Expected text to not be null or empty but found {Describe(text)}",
            "Text is not null or empty");
    }
}