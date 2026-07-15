using Moq;
using TSpec.Assert;
using TSpec.Test.Subjects.Order;
using TSpec.Test.Subjects.Purchase;

namespace TSpec.Test.Tests.PurchaseHandler;

public class WhenPurchase : Spec<Subjects.Purchase.PurchaseHandler, PurchaseResponseModel>
{
    protected WhenPurchase() 
        => When(_ => _.Purchase(An<int>()))
        .Given<ICheckoutProvider>().That(_ => _.GetExistingCheckout(The<int>())).Returns(A<Checkout>)
        .And<IBasketRepository>().That(_ => _.GetEditable(The<int>())).Returns(() => The<Checkout>().Basket);

    public class GivenEditableBasket : WhenPurchase
    {
        public GivenEditableBasket() => Given<Checkout>(_ => _.IsOpen = true);

        [Fact]
        public void ThenPublishBasketPurchasedEventAndCheckoutIsClosed()
        {
            Then<ITopicExchangeV2<BasketPurchasedV1>>(_ => _.Publish(It.IsAny<BasketPurchasedV1>()))
            .And(The<Checkout>()).IsOpen.Is().False();
            Specification.Is(
                """
                Given Checkout has IsOpen = true
                  and ICheckoutProvider.GetExistingCheckout(the int) returns a Checkout
                  and IBasketRepository.GetEditable(the int) returns the Checkout's Basket
                When _.Purchase(an int)
                Then ITopicExchangeV2<BasketPurchasedV1>.Publish(It.IsAny<BasketPurchasedV1>())
                  and the Checkout's IsOpen is false
                """);
        }
    }
}