using TSpec.Assert;
using TSpec.Test.Subjects;

namespace TSpec.Test.Tests.ShoppingServiceAsync;

public abstract class WhenPlaceOrder : Spec<Subjects.ShoppingServiceAsync, object>
{
    protected ShoppingCart _cart = null!;

    protected WhenPlaceOrder() => When(_ => _.PlaceOrder(_cart!));

    public class GivenOpenCart : WhenPlaceOrder
    {
        public GivenOpenCart() => Given().That(() => _cart = new() { IsOpen = true });

        [Fact]
        public void ThenOrderIsCreated()
        {
            Then<IOrderService>(_ => _.CreateOrder(_cart));
            Specification.Is(
                """
                Given that _cart = new() { IsOpen = true }
                When _.PlaceOrder(_cart)
                Then IOrderService.CreateOrder(_cart)
                """);
        }
    }

    public class GivenClosedCart : WhenPlaceOrder
    {
        public GivenClosedCart() => Given().That(() => _cart = new() { IsOpen = false });

        [Fact]
        public void ThenThrowsNotPurchasable()
        {
            Then().Throws<NotPurchasable>();
            Specification.Is(
                """
                Given that _cart = new() { IsOpen = false }
                When _.PlaceOrder(_cart)
                Then throws NotPurchasable
                """);
        }
    }
}