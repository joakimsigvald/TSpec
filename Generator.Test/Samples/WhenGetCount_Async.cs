namespace TSpec.Generator.Test.Samples;

public abstract class WhenGetCount_Async : Spec<object, int?>
{
    [Fact]
    public async Task ThenItIs42()
    {
        await Task.Yield();
        Result.Is(42);
    }
}
