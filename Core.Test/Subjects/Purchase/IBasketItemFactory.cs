using TSpec.Test.Subjects.Shopping;

namespace TSpec.Test.Subjects.Purchase;

public interface IBasketItemFactory
{
    Task<BasketItem[]> CreateBasketItems(int customerId, int companyId);
}