using TSpec.Assert;
using TSpec.Test.Subjects;

namespace TSpec.Test.Tests.ShoppingServiceAsync;

public abstract class WhenRemoveItem : Spec<Subjects.ShoppingServiceAsync, ShoppingCart>
{
    protected int CartId = 123;
    protected ShoppingCartItem[] CartItems = null!;
    protected readonly ShoppingCartItem Item = new("X");
    private ShoppingCart _cart = null!;

    protected WhenRemoveItem()
        => When(_ => _.RemoveFromCart(CartId, Cart.Items[0]))
        .Given<IShoppingCartRepository>().That(_ => _.GetCart(CartId))
        .Returns(() => new ShoppingCart { Id = CartId, Items = CartItems! });

    protected ShoppingCart Cart => _cart ??= new() { Id = CartId, Items = CartItems };

    public class GivenCartWithOneItem : WhenRemoveItem
    {
        public GivenCartWithOneItem() => Given().That(() => CartItems = [new ShoppingCartItem("X")]);

        [Fact]
        public void ThenCartIsEmpty()
        {
            Result.Items.Is().Empty();
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems! }
                  and that CartItems = [new ShoppingCartItem("X")]
                When _.RemoveFromCart(CartId, Cart.Items[0])
                Then Result.Items is empty
                """);
        }
    }
}