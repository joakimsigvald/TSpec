using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.IsEnumerable;

public class WhenNull : Spec
{
    [Fact]
    public void GivenNull_ThenDoesNotThrow() => ((int[]?)null).Is().Null().and.Null();

    [Fact]
    public void GivenNotNull_ThenGetException()
    {
        var arr = Zero<int>();
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().Null());
        ex.HasMessage($"Expected arr to be null but found []", "Arr is null");
    }
}