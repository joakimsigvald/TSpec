using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Time.IsNullableDateTime;

public class WhenAfter : Spec
{
    [Fact]
    public void GivenAfter_ThenDoesNotThrow()
        => A<DateTime?>().Is().After(The<DateTime?>()!.Value.AddDays(-1))
        .and.CloseTo(The<DateTime?>()!.Value, TimeSpan.Zero);

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var a = A<DateTime?>();
        var b = a!.Value.AddDays(1);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().After(b));
        ex.HasMessage($"Expected a to occur after {b} but found {a}", "A is after b");
    }
}