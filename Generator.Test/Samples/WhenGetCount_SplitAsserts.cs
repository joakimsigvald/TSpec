namespace TSpec.Generator.Test.Samples;

public abstract class WhenGetCount_SplitAsserts : Spec<object, int?>
{
    [Fact] public void ThenItIsNotNull() => Result.Is().Not(null);

    [Fact] public void ThenItIs42() => Result.Is(42);
}
