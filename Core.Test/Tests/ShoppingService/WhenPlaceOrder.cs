using TSpec.Test.Subjects;
using Moq;
using TSpec.Assert;

namespace TSpec.Test.Tests.ShoppingService;

public class WhenPlaceOrder : ShoppingServiceSpec<object>
{
    public WhenPlaceOrder() => When(_ => _.PlaceOrder(A<ShoppingCart>()));

    [Theory]
    [InlineData(2)]
    [InlineData(123)]
    public void ThenOrderIsCreated(int shopId)
    {
        Using(shopId)
            .Then<IOrderService>(_ => _.CreateOrder(The<ShoppingCart>()))
            .And<ILogger>(_ => _.ForContext("ShopId", shopId));
        Specification.Is(
            """
            Using shopId
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService.CreateOrder(the ShoppingCart)
              and ILogger.ForContext("ShopId", shopId)
            """);
    }

    [Fact]
    public void ThenLogOrderCreated_With_ShopNameAndDivision()
    {
        Using((A<string>(), ASecond<string>()))
            .Then<ILogger>(_ => _.Information(
                It.Is<string>(s => s.Contains(A<string>()) && s.Contains(ASecond<string>()))));
        Specification.Is(
            """
            Using (a string, a second string)
            When _.PlaceOrder(a ShoppingCart)
            Then ILogger.Information(It.Is<string>(s.Contains(a string) && s.Contains(a
                  second string)))
            """);
    }
}