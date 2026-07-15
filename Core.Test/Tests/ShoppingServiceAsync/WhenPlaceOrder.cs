using TSpec.Assert;
using TSpec.Test.Subjects;

namespace TSpec.Test.Tests.ShoppingServiceAsync;

public abstract class WhenPlaceOrder : Spec<Subjects.ShoppingServiceAsync, object>
{
    protected ShoppingCart Cart = null!;

    protected WhenPlaceOrder() => When(_ => _.PlaceOrder(Cart!));

    public class GivenOpenCart : WhenPlaceOrder
    {
        public GivenOpenCart() => Given().That(() => Cart = new() { IsOpen = true });

        [Fact]
        public void ThenOrderIsCreated()
        {
            Then<IOrderService>(_ => _.CreateOrder(Cart));
            Specification.Is(
                """
                Given that Cart = new() { IsOpen = true }
                When _.PlaceOrder(Cart!)
                Then IOrderService.CreateOrder(Cart)
                """);
        }
    }

    public class GivenClosedCart : WhenPlaceOrder
    {
        public GivenClosedCart() => Given().That(() => Cart = new() { IsOpen = false });

        [Fact]
        public void ThenThrowsNotPurchasable()
        {
            Then().Throws<NotPurchasable>();
            Specification.Is(
                """
                Given that Cart = new() { IsOpen = false }
                When _.PlaceOrder(Cart!)
                Then throws NotPurchasable
                """);
        }
    }
}