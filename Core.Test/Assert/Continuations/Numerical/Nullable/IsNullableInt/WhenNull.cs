using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Numerical.Nullable.IsNullableInt;

public class WhenNull : Spec
{
    [Fact] public void GivenNull_ThenDoesNotThrow() => ((int?)null).Is().Null();

    [Fact]
    public void GivenFail_ThenGetException()
    {
        int? x = 1;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => x.Is().Null());
        ex.HasMessage("Expected x to be null but found 1", "X is null");
    }
}