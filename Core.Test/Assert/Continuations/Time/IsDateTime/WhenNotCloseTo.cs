using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Time.IsDateTime;

public class WhenNotCloseTo : Spec
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(-2, 1)]
    public void GivenNotCloseTo_ThenDoesNotThrow(int days, int toleranceDays)
        => A<DateTime>().Is().not.CloseTo(The<DateTime>().AddDays(days), TimeSpan.FromDays(toleranceDays));

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var a = A<DateTime>();
        var b = a.AddDays(1);
        var tolerance = TimeSpan.FromDays(1);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().not.CloseTo(b, tolerance));
        ex.HasMessage($"Expected a to not be close to {b} but found {a}", "A is not close to b");
    }
}