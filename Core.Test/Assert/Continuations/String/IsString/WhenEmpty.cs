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
    public void GivenNotEmpty_ThenGetException(string? text)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Is().Empty());
        ex.HasMessage($"Expected text to be empty but found {Describe(text)}", "Text is empty");
    }
}