using TSpec.Assert;
using TSpec.Test.Subjects;

namespace TSpec.Test.Tests.ShoppingServiceAsync;

public abstract class WhenAddItem : Spec<Subjects.ShoppingServiceAsync, ShoppingCart>
{
    protected int CartId = 123;
    protected ShoppingCartItem[] CartItems = null!;
    protected readonly ShoppingCartItem NewItem = new("N1");

    protected WhenAddItem()
        => Given<IShoppingCartRepository>()
        .That(_ => _.GetCart(CartId)).Returns(() => new ShoppingCart { Id = CartId, Items = CartItems ?? [] })
        .When(_ => _.AddToCart(CartId, NewItem));

    public class GivenEmptyCart : WhenAddItem
    {
        [Fact]
        public void ThenCartItemsIsNotNull()
        {
            Result.Items.Is().not.Null();
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                When _.AddToCart(CartId, NewItem)
                Then Result.Items is not null
                """);
        }

        [Fact]
        public void ThenCartItemsIsNotEmpty()
        {
            Result.Items.Is().not.Empty();
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                When _.AddToCart(CartId, NewItem)
                Then Result.Items is not empty
                """);
        }

        [Fact]
        public void TheIdIsPreserved()
        {
            Result.Id.Is(CartId);
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                When _.AddToCart(CartId, NewItem)
                Then Result.Id is CartId
                """);
        }

        [Fact]
        public void ThenCartIsStored()
        {
            Then<IShoppingCartRepository>(_ => _.StoreCart(Result));
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                When _.AddToCart(CartId, NewItem)
                Then IShoppingCartRepository.StoreCart(Result)
                """);
        }
    }

    public class GivenCartWithOneItem : WhenAddItem
    {
        public GivenCartWithOneItem() => Given().That(() => CartItems = [new ShoppingCartItem("A1")]);

        [Fact]
        public void ThenDoNotThrow()
        {
            Then().DoesNotThrow();
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                  and that CartItems = [new ShoppingCartItem("A1")]
                When _.AddToCart(CartId, NewItem)
                Then does not throw
                """);
        }

        [Fact]
        public void ThenCartHasTwoItems()
        {
            Result.Items.Length.Is(2);
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                  and that CartItems = [new ShoppingCartItem("A1")]
                When _.AddToCart(CartId, NewItem)
                Then Result.Items.Length is 2
                """);
        }

        [Fact]
        public void ThenNewItemIsLast()
        {
            Result.Items.Last().Sku.Is(NewItem.Sku);
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                  and that CartItems = [new ShoppingCartItem("A1")]
                When _.AddToCart(CartId, NewItem)
                Then Result.Items.Last().Sku is NewItem.Sku
                """);
        }

        [Fact]
        public void ThenNewItemIsCloned()
        {
            Result.Items.Last().Is().Not(NewItem);
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                  and that CartItems = [new ShoppingCartItem("A1")]
                When _.AddToCart(CartId, NewItem)
                Then Result.Items.Last() is not NewItem
                """);
        }

        [Fact]
        public void ThenItemsAreNotNull()
        {
            Result.Items.Has().All(it => it != null);
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                  and that CartItems = [new ShoppingCartItem("A1")]
                When _.AddToCart(CartId, NewItem)
                Then Result.Items has all it != null
                """);
        }

        [Fact]
        public void ThenItemsHaveLineNumbers()
        {
            Result.Items.Has().All((it, i) => it.LineNumber == i + 1);
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                  and that CartItems = [new ShoppingCartItem("A1")]
                When _.AddToCart(CartId, NewItem)
                Then Result.Items has all (it, i) => it.LineNumber == i + 1
                """);
        }

        [Fact]
        public void ThenItemsAssertNotNull()
        {
            Result.Items.Has().All(it => it.Is().not.Null());
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                  and that CartItems = [new ShoppingCartItem("A1")]
                When _.AddToCart(CartId, NewItem)
                Then Result.Items has all it.Is().not.Null()
                """);
        }

        [Fact]
        public void ThenItemsAssertHaveLineNumbers()
        {
            Result.Items.Has().All((it, i) => it.LineNumber.Is(i + 1));
            Specification.Is(
                """
                Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
                      CartId, Items = CartItems ?? [] }
                  and that CartItems = [new ShoppingCartItem("A1")]
                When _.AddToCart(CartId, NewItem)
                Then Result.Items has all (it, i) => it.LineNumber.Is(i + 1)
                """);
        }
    }
}