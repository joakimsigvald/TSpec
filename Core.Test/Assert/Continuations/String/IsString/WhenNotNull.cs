using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNotNull : StringSpec
{
    [Fact]
    public void GivenNotNull_ThenDoesNotThrow()
        => "".Is().not.Null().and.not.Null();

    [Fact]
    public void GivenNull_ThenGetException()
    {
        string? str = null;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => str.Is().not.Null());
        ex.HasMessage($"Expected str to not be null but found {Describe(str)}", "Str is not null");
    }
}