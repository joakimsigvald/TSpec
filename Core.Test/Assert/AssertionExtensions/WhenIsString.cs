using TSpec.Assert;

namespace TSpec.Test.Assert.AssertionExtensions;

public class WhenIsString : Spec<string>
{
    [Fact] public void GivenSame_ThenDoesNotThrow() => When(_ => _.Is(_)).Then();

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => Using("abcd").When(_ => _).Then().Result.Is("abce"));
        ex.HasMessage(
            """
            Assert.Equal() Failure: Strings differ
                          ↓ (pos 3)
            Expected: "abce"
            Actual:   "abcd"
                          ↑ (pos 3)
            """);
    }
}
