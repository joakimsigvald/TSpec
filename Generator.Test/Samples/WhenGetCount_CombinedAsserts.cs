namespace TSpec.Generator.Test.Samples;

public abstract class WhenGetCount_CombinedAsserts : Spec<object, int?>
{
    [Fact]
    public void ThenItIs42()
    {
        Result.Is().Not(null);
        Result.Is(42);
    }
}
