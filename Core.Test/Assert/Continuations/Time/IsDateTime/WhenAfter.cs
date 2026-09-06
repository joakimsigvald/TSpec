using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Assert.Continuations.Time.IsDateTime;

public class WhenAfter : Spec
{
    [Fact] public void GivenAfter_ThenDoesNotThrow() => A<DateTime>().Is().After(The<DateTime>().AddDays(-1));

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var a = A<DateTime>();
        var b = a.AddDays(1);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().After(b));
        ex.HasMessage($"Expected a to occur after {b.InvariantText()} but found {a.InvariantText()}", "A is after b");
    }
}