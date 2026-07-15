using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Time.IsDateTime;

public class WhenBefore : Spec
{
    [Fact] public void GivenBefore_ThenDoesNotThrow() => A<DateTime>().Is().Before(The<DateTime>().AddDays(1));

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var a = A<DateTime>();
        var b = a.AddDays(-1);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().Before(b));
        ex.HasMessage($"Expected a to occur before {b} but found {a}", "A is before b");
    }
}