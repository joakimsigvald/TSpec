using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Time.IsTimeSpan;

public class WhenNotNegative : Spec
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void GivenNotNegative_ThenDoesNotThrow(int days)
        => TimeSpan.FromDays(days).Is().not.Negative();

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var a = TimeSpan.FromDays(-1);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().not.Negative());
        ex.HasMessage($"Expected a to not be negative but found {a}", "A is not negative");
    }
}