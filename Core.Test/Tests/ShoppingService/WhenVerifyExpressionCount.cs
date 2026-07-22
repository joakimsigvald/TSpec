using Moq;
using static Moq.Times;
using TSpec.Assert;
using TSpec.Test.Subjects;
using Xunit.Sdk;

namespace TSpec.Test.Tests.ShoppingService;

public class WhenVerifyExpressionCountPlaceOrder : ShoppingServiceSpec<object>
{
    public WhenVerifyExpressionCountPlaceOrder() => When(_ => _.PlaceOrder(A<ShoppingCart>()));

    [Fact]
    public void ThenExpressionWithCountRendersInvocationPhrase()
    {
        Then<IOrderService>(_ => _.CreateOrder(The<ShoppingCart>()), Once);
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService.CreateOrder(the ShoppingCart) was invoked once
            """);
    }

    [Fact]
    public void ThenBareExpressionRendersJustTheCall()
    {
        Then<IOrderService>(_ => _.CreateOrder(The<ShoppingCart>()));
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService.CreateOrder(the ShoppingCart)
            """);
    }
}

public class WhenVerifyExpressionCountCreateCart : Spec<Subjects.ShoppingService, ShoppingCart>
{
    public WhenVerifyExpressionCountCreateCart() => When(_ => _.CreateCart(An<int>()));

    [Fact]
    public void ThenNegativeExpressionRendersWasNotInvoked()
    {
        Then<IOrderService>(_ => _.CreateOrder(The<ShoppingCart>()), Never);
        Specification.Is(
            """
            When _.CreateCart(an int)
            Then IOrderService.CreateOrder(the ShoppingCart) was not invoked
            """);
    }

    [Fact]
    public void ThenExpressionOnceFailsWhenNeverCalled()
        => Xunit.Assert.Throws<MockException>(
            () => Then<IOrderService>(_ => _.CreateOrder(The<ShoppingCart>()), Once));
}
