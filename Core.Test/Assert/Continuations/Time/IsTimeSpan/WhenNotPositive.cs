using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Time.IsTimeSpan;

public class WhenNotPositive : Spec
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNotPositive_ThenDoesNotThrow(int days)
        => TimeSpan.FromDays(days).Is().not.Positive();

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var a = TimeSpan.FromDays(1);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().not.Positive());
        ex.HasMessage($"Expected a to not be positive but found {a}", "A is not positive");
    }
}