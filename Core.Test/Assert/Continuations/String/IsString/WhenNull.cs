using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNull : StringSpec
{
    [Fact]
    public void GivenNull_ThenDoesNotThrow()
        => ((string?)null).Is().Null().and.Null();

    [Fact]
    public void GivenNotNull_ThenGetException()
    {
        var str = "";
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => str.Is().Null());
        ex.HasMessage($"Expected str to be null but found {Describe(str)}", "Str is null");
    }
}