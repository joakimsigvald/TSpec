namespace TSpec.Generator.Test.Samples;

public abstract class WhenGetCount_NonConstantArgument : Spec<object, int?>
{
    [Fact] public void ThenItIsTheInt() => Result.Is(A<int>());
}
