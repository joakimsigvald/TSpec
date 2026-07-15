using TSpec.Test.Subjects.Shopping;

namespace TSpec.Test.Subjects.Purchase;

public interface IBasketRepository
{
    Task<Basket> GetEditable(int basketId);
    Task<Basket> UpdateStatus(int basketId);
}