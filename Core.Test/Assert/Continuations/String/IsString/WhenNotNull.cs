using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.IsString;

public class WhenNotNull : StringSpec
{
    [Fact]
    public void GivenNotNull_ThenCompletes()
        => "".Is().not.Null().and.not.Null();

    [Fact]
    public void GivenNull_ThenGetException()
    {
        string? str = null;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => str.Is().not.Null());
        ex.HasMessage($"Expected str to not be null but it was {Describe(str)}", "Str is not null");
    }
}