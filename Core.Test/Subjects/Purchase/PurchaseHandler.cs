namespace TSpec.Test.Subjects.Purchase;

public class PurchaseHandler(
    IBasketRepository basketRepository,
    ICheckoutProvider checkoutProvider,
    ITopicExchangeV2<BasketPurchasedV1> basketPurchased)
{
    public async Task<PurchaseResponseModel> Purchase(int basketId)
    {
        var checkout = await checkoutProvider.GetExistingCheckout(basketId);
        if (!checkout.IsOpen)
            throw new InvalidOperationException("Checkout must be open");
        checkout.IsOpen = false;
        await basketRepository.UpdateStatus(basketId);
        var ev = new BasketPurchasedV1();
        await basketPurchased.Publish(ev);
        return new PurchaseResponseModel();
    }
}