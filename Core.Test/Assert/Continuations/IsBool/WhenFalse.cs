using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsBool;

public class WhenFalse : Spec
{
    [Fact]
    public void GivenFalse_ThenDoesNotThrow()
        => false.Is().False().and.False();

    [Fact]
    public void GivenTrue_ThenGetException()
    {
        var actual = true;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().False());
        ex.HasMessage($"Expected actual to be false but found true", "Actual is false");
    }
}