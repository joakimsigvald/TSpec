namespace TSpec.Test.Subjects;

public class CounterService(ICounterStore store)
{
    private int _offset;

    public int Offset => _offset;

    public void SetOffset(int offset) => _offset = offset;

    public async ValueTask<int> IncrementAndGet(string key)
    {
        await store.Increment(key);
        return await store.GetCount(key) + _offset;
    }

    public ValueTask Increment(string key) => store.Increment(key);

    public async ValueTask<int> GetCartId(int id) => (await store.GetCart(id)).Id;
}
