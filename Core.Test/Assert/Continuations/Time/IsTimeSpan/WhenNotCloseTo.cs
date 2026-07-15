using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Time.IsTimeSpan;

public class WhenNotCloseTo : Spec
{
    [Fact]
    public void GivenNotCloseTo_ThenDoesNotThrow()
        => A<TimeSpan>().Is().not.CloseTo(The<TimeSpan>().Add(TimeSpan.FromDays(1)), TimeSpan.Zero);

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    public void GivenFail_ThenGetException(int days, int toleranceDays)
    {
        var a = A<TimeSpan>();
        var b = a.Add(TimeSpan.FromDays(days));
        var tolerance = TimeSpan.FromDays(toleranceDays);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().not.CloseTo(b, tolerance));
        ex.HasMessage($"Expected a to not be close to {b} but found {a}", "A is not close to b");
    }
}