using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNotNullOrWhitespace : StringSpec
{
    [Fact]
    public void GivenNotNullOrWhitespace_ThenDoesNotThrow()
        => "abc".Is().not.NullOrWhitespace().and.not.NullOrWhitespace();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenNullOrWhitespace_ThenGetException(string? text)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Is().not.NullOrWhitespace());
        ex.HasMessage($"Expected text to not be null or whitespace but found {Describe(text)}",
            "Text is not null or whitespace");
    }
}