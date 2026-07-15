using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsObject;

public class WhenEquivalentTo : Spec
{
    [Fact]
    public void GivenEquivalentTo_ThenDoesNotThrow()
        => new MyRecord("abc").Is().EquivalentTo(new MyOtherRecord("abc")).and.not.Null();

    [Fact]
    public void GivenNotEquivalentTo_ThenGetException()
    {
        var actual = new MyRecord("abc");
        var expected = new MyOtherRecord("def");
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().EquivalentTo(expected));
        ex.HasMessage($"Expected actual to be equivalent to {expected} but found {actual}", "Actual is equivalent to expected");
    }
}