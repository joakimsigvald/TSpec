using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.IsEnumerable;

public class WhenNotNull : Spec
{
    [Fact]
    public void GivenNotNull_ThenDoesNotThrow() => Zero<int>().Is().not.Null().and.Empty();

    [Fact]
    public void GivenNull_ThenGetException()
    {
        int[]? arr = null;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().not.Null());
        ex.HasMessage($"Expected arr to not be null but found null", "Arr is not null");
    }
}