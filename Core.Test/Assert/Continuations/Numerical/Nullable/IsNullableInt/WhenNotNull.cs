using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Numerical.Nullable.IsNullableInt;

public class WhenNotNull : Spec
{
    [Fact] public void GivenNotNull_ThenDoesNotThrow() => ((int?)1).Is().not.Null().and.LessThan(2);

    [Fact]
    public void GivenFail_ThenGetException()
    {
        int? x = null;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => x.Is().not.Null());
        ex.HasMessage("Expected x to not be null but found null", "X is not null");
    }
}