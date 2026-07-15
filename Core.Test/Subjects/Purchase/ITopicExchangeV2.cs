namespace TSpec.Test.Subjects.Purchase;

public interface ITopicExchangeV2<T>
{
    Task Publish(BasketPurchasedV1 basketPurchasedV1);
}