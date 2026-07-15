using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Time.IsNullableDateTime;

public class WhenBefore : Spec
{
    [Fact]
    public void GivenBefore_ThenDoesNotThrow()
        => A<DateTime?>().Is().Before(The<DateTime?>()!.Value.AddDays(1))
        .and.CloseTo(The<DateTime?>()!.Value, TimeSpan.Zero);

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var a = A<DateTime?>();
        var b = a!.Value.AddDays(-1);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().Before(b));
        ex.HasMessage($"Expected a to occur before {b} but found {a}", "A is before b");
    }
}