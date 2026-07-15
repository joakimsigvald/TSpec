using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsBool;

public class WhenTrue : Spec
{
    [Fact]
    public void GivenTrue_ThenDoesNotThrow()
        => true.Is().True().and.True();

    [Fact]
    public void GivenFalse_ThenGetException()
    {
        var actual = false;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().True());
        ex.HasMessage($"Expected actual to be true but found false", "Actual is true");
    }
}