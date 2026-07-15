using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Time.IsNullableTimeSpan;

public class WhenNull : Spec
{
    [Fact] public void GivenNull_ThenDoesNotThrow() => ((TimeSpan?)null).Is().Null();

    [Fact]
    public void GivenFail_ThenGetException()
    {
        TimeSpan? x = TimeSpan.Zero;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => x.Is().Null());
        ex.HasMessage("Expected x to be null but found 00:00:00", "X is null");
    }
}