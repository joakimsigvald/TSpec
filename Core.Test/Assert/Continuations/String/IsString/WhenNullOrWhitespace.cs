using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNullOrWhitespace : StringSpec
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenNullOrWhitespace_ThenDoesNotThrow(string? text)
        => text.Is().NullOrWhitespace().and.NullOrWhitespace();

    [Fact]
    public void GivenNotNullOrWhitespace_ThenGetException()
    {
        var str = "abc";
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => str.Is().NullOrWhitespace());
        ex.HasMessage($"Expected str to be null or whitespace but found {Describe(str)}", "Str is null or whitespace");
    }
}