using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Assert.Continuations.Time.IsNullableDateTime;

public class WhenNotBefore : Spec
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNotBefore_ThenDoesNotThrow(int days)
        => A<DateTime?>().Is().not.Before(The<DateTime?>()!.Value.AddDays(days))
        .and.CloseTo(The<DateTime?>()!.Value, TimeSpan.Zero);

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var a = A<DateTime?>();
        var b = a!.Value.AddDays(1);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().not.Before(b));
        ex.HasMessage($"Expected a to not occur before {b.InvariantText()} but found {a.InvariantText()}", "A is not before b");
    }
}