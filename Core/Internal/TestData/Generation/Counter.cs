namespace TSpec.Internal.TestData.Generation;

internal class Counter
{
    private int _counter = 0;
    public int Next => ++_counter;
    public int Current => _counter;
}