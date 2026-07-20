using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNotEmpty : StringSpec
{
    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    public void GivenNotEmpty_ThenDoesNotThrow(string? text)
        => text.Is().not.Empty().and.not.Empty();

    [Theory]
    [InlineData("")]
    public void GivenEmpty_ThenGetException(string text)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Is().not.Empty());
        ex.HasMessage($"Expected text to not be empty but found {Describe(text)}", "Text is not empty");
    }
}