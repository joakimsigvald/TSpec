namespace TSpec.Generator.Test.Samples;

public static class Store
{
    public static int Count;
}

public abstract class WhenGetCount_AssertOnAStatic : Spec<object, int?>
{
    [Fact]
    public void ThenItIsStored()
    {
        Result.Is(42);
        Store.Count.Is(1);
    }
}
