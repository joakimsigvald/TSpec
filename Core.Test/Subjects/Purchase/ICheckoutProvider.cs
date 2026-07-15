using TSpec.Test.Subjects.Order;

namespace TSpec.Test.Subjects.Purchase;

public interface ICheckoutProvider
{
    Task<Checkout> GetExistingCheckout(int basketId);
}