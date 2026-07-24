namespace TSpec.Test.Subjects;

public interface ICounterStore
{
    ValueTask<int> GetCount(string key);
    ValueTask Increment(string key);
    ValueTask<ShoppingCart> GetCart(int id);
}
