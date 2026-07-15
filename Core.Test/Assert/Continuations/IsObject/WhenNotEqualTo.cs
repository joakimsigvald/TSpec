using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsObject;

public class WhenNotEqualTo : Spec
{
    [Fact]
    public void GivenNotEqual_ThenDoesNotThrow()
        => new MyRecord("abc").Is().not.EqualTo(new MyRecord("def")).and.not.Null();

    [Fact]
    public void GivenEqual_ThenGetException()
    {
        var actual = new MyRecord("abc");
        var expected = new MyRecord("abc");
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().not.EqualTo(expected));
        ex.HasMessage($"Expected actual to not be equal to {expected} but found {actual}",
            "Actual is not equal to expected");
    }
}