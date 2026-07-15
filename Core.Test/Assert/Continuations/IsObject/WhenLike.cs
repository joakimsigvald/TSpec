using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsObject;

public class WhenLike : Spec
{
    [Fact]
    public void GivenLike_ThenDoesNotThrow()
        => new MyRecord("abc").Is().Like(new MyOtherRecord("abc")).and.not.Null();

    [Fact]
    public void GivenNotLike_ThenGetException()
    {
        var actual = new MyRecord("abc");
        var expected = new MyOtherRecord("def");
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().Like(expected));
        ex.HasMessage($"Expected actual to be like {expected} but found {actual}", "Actual is like expected");
    }
}