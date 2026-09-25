using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsObject;

public class WhenSameAs : Spec
{
    [Fact]
    public void GivenSame_ThenCompletes()
    {
        var actual = new MyRecord("abc");
        actual.Is().SameAs(actual);
        Specification.Is("Actual is same as actual");
    }

    [Fact]
    public void GivenEqualButNotSame_ThenGetException()
    {
        var actual = new MyRecord("abc");
        var expected = new MyRecord("abc");
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().SameAs(expected));
        ex.HasMessage($"Expected actual to be same as {expected} but found {actual}", "Actual is same as expected");
    }
}
