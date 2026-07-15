using TSpec.Assert;
using TSpec.Test.Subjects.Shopping;

namespace TSpec.Test.Tests.BasketItemFactory;

public class WhenCreateBasketItems : Spec<Subjects.Shopping.BasketItemFactory, BasketItem[]>
{
    protected WhenCreateBasketItems() 
        => When(_ => _.CreateBasketItems(An<int>(), ASecond<int>(), A<NewBasketItem[]>()));

    public class GivenItemWithUnknownProduct : WhenCreateBasketItems
    {
        public GivenItemWithUnknownProduct()
            => Given(One<NewBasketItem>());

        [Fact]
        public void ThenThrowBasketItemNotBuyable()
        {
            Then().Throws<BasketItemNotBuyable>();
            Specification.Is(
                """
                Given one NewBasketItem
                When _.CreateBasketItems(an int, a second int, a NewBasketItem[])
                Then throws BasketItemNotBuyable
                """);
        }
    }
}