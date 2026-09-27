namespace TSpec.Generator.Test.Samples;

public abstract class WhenGetCount_ProtectedGiven : Spec<object, int?>
{
    protected abstract class GivenNothing : WhenGetCount_ProtectedGiven
    {
        public abstract class AndNothingElse : GivenNothing
        {
            [Fact] public void ThenItIs42() => Result.Is(42);
        }
    }
}
