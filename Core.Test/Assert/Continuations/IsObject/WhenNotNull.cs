using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsObject;

public class WhenNotNull : Spec
{
    [Fact]
    public void GivenNotNull_ThenDoesNotThrow()
        => new object().Is().not.Null().and.not.Null();

    [Fact]
    public void GivenNull_ThenGetException()
    {
        object? actual = null;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().not.Null());
        ex.HasMessage($"Expected actual to not be null but found null", "Actual is not null");
    }
}