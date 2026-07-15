using TSpec.Assert;
using TSpec.Test.Subjects.Order;

namespace TSpec.Test.Tests.PurchaseOrderFactory;

public class WhenCreateOrder : PurchaseOrderFactorySpec<OrderRecord>
{
    protected const int BasketId = 123;
    protected const int CompanyId = 234;
    protected Checkout Checkout = new() { Basket = new() };

    protected WhenCreateOrder() => When(_ => _.CreateOrder(Checkout));

    public class GivenBasket : WhenCreateOrder
    {
        public GivenBasket() => Given().That(() => Checkout = new() { Basket = new() { Id = BasketId } });

        [Fact]
        public void ThenQuotationId_Is_BasketId()
        {
            Result.QuotationId.Is(BasketId);
            Specification.Is(
                """
                Given that Checkout = new() { Basket = new() { Id = BasketId } }
                When _.CreateOrder(Checkout)
                Then Result.QuotationId is BasketId
                """);
        }

        [Fact]
        public void ThenOrderNo_Is_BasketId()
        {
            Result.OrderNo.Is($"{BasketId}");
            Specification.Is(
                """
                Given that Checkout = new() { Basket = new() { Id = BasketId } }
                When _.CreateOrder(Checkout)
                Then Result.OrderNo is "{BasketId}"
                """);
        }
    }
}