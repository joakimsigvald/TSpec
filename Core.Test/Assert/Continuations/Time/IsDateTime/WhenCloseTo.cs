using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Time.IsDateTime;

public class WhenCloseTo : Spec
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    public void GivenCloseTo_ThenDoesNotThrow(int days, int toleranceDays)
        => A<DateTime>().Is().CloseTo(The<DateTime>().AddDays(days), TimeSpan.FromDays(toleranceDays));

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var a = A<DateTime>();
        var b = a.AddDays(1);
        var tolerance = TimeSpan.FromDays(0);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().CloseTo(b, tolerance));
        ex.HasMessage($"Expected a to be close to {b} but found {a}", "A is close to b");
    }
}