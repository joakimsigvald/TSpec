using Moq;
using static Moq.Times;
using TSpec.Assert;
using TSpec.Test.Subjects;
using Xunit.Sdk;

namespace TSpec.Test.Tests.ShoppingService;

public class WhenPlaceOrderInvocations : ShoppingServiceSpec<object>
{
    public WhenPlaceOrderInvocations() => When(_ => _.PlaceOrder(A<ShoppingCart>()));

    [Fact]
    public void ThenOrderServiceWasInvoked()
    {
        Then<IOrderService>(wasInvoked: AtLeastOnce);
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService was invoked
            """);
    }

    [Fact]
    public void ThenOrderServiceWasInvokedOnce()
    {
        Then<IOrderService>(wasInvoked: Once);
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService was invoked once
            """);
    }

    [Fact]
    public void ThenOrderServiceWasInvokedExactlyAndAtMost()
    {
        Then<IOrderService>(wasInvoked: Exactly(1))
            .And<IOrderService>(wasInvoked: AtMost(2));
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService was invoked Exactly(1)
              and IOrderService was invoked AtMost(2)
            """);
    }

    [Fact]
    public void ThenWasInvokedComposesWithSpecificVerification()
    {
        Then<IOrderService>(wasInvoked: Once)
            .And<IOrderService>(_ => _.CreateOrder(The<ShoppingCart>()));
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService was invoked once
              and IOrderService.CreateOrder(the ShoppingCart)
            """);
    }

    [Fact]
    public void ThenSpecificVerificationComposesWithWasInvoked()
    {
        Then<IOrderService>(_ => _.CreateOrder(The<ShoppingCart>()))
            .And<IOrderService>(wasInvoked: Once);
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService.CreateOrder(the ShoppingCart)
              and IOrderService was invoked once
            """);
    }

    [Fact]
    public void ThenWasInvokedNeverFails()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => Then<IOrderService>(wasInvoked: Never));
        ex.Message.Is("Expected IOrderService to be invoked never but was invoked 1 times");
    }

    [Fact]
    public void ThenWasInvokedAtLeastTwiceFails()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => Then<IOrderService>(wasInvoked: AtLeast(2)));
        ex.Message.Is("Expected IOrderService to be invoked AtLeast(2) but was invoked 1 times");
    }
}

public class WhenCreateCartInvocations : Spec<Subjects.ShoppingService, ShoppingCart>
{
    public WhenCreateCartInvocations() => When(_ => _.CreateCart(An<int>()));

    [Fact]
    public void ThenOrderServiceWasNotInvoked()
    {
        Then<IOrderService>(wasInvoked: Never);
        Specification.Is(
            """
            When _.CreateCart(an int)
            Then IOrderService was not invoked
            """);
    }

    [Fact]
    public void ThenLoggerWasNotInvoked()
        => Then<ILogger>(wasInvoked: Never);

    [Fact]
    public void ThenWasInvokedOnceFailsWhenNeverCalled()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => Then<IOrderService>(wasInvoked: Once));
        ex.Message.Is("Expected IOrderService to be invoked once but was invoked 0 times");
    }
}
