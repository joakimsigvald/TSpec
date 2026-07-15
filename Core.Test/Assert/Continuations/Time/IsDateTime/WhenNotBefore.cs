using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Time.IsDateTime;

public class WhenNotBefore : Spec
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNotBefore_ThenDoesNotThrow(int days)
        => A<DateTime>().Is().not.Before(The<DateTime>().AddDays(days));

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var a = A<DateTime>();
        var b = a.AddDays(1);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().not.Before(b));
        ex.HasMessage($"Expected a to not occur before {b} but found {a}", "A is not before b");
    }
}