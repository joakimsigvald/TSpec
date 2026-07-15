using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsObject;

public class WhenNotLike : Spec
{
    [Fact]
    public void GivenNotLike_ThenDoesNotThrow()
        => new MyRecord("abc").Is().not.Like(new MyOtherRecord("def")).and.not.Null();

    [Fact]
    public void GivenLike_ThenGetException()
    {
        var actual = new MyRecord("abc");
        var expected = new MyOtherRecord("abc");
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().not.Like(expected));
        ex.HasMessage($"Expected actual to not be like {expected} but found {actual}", "Actual is not like expected");
    }
}