using TSpec.Assert;
using TSpec.Test.Subjects;

namespace TSpec.Test.Tests.ShoppingServiceAsync;

public abstract class WhenRemoveItem : Spec<Subjects.ShoppingServiceAsync, ShoppingCart>
{
    protected const int CartId = 123;
    protected ShoppingCartItem[] _cartItems = null!;
    protected readonly ShoppingCartItem _item = new("X");
    private ShoppingCart _cart = null!;

    protected WhenRemoveItem()
        => When(_ => _.RemoveFromCart(CartId, Cart.Items[0]))
        .Given<IShoppingCartRepository>().That(_ => _.GetCart(CartId))
        .Returns(() => new ShoppingCart { Id = CartId, Items = _cartItems! });

    protected ShoppingCart Cart => _cart ??= new() { Id = CartId, Items = _cartItems };

    public class GivenCartWithOneItem : WhenRemoveItem
    {
        public GivenCartWithOneItem() => Given().That(() => _cartItems = [new ShoppingCartItem("X")]);

        [Fact]
        public void ThenCartIsEmpty()
        {
            Result.Items.Is().Empty();
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = _cartItems }
                  and that _cartItems = [new ShoppingCartItem("X")]
                When _.RemoveFromCart(CartId, Cart.Items[0])
                Then Result.Items is empty
                """);
        }
    }
}