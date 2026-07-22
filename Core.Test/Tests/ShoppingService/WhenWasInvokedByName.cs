using static Moq.Times;
using TSpec.Assert;
using TSpec.Test.Subjects;
using Xunit.Sdk;

namespace TSpec.Test.Tests.ShoppingService;

public class WhenPlaceOrderInvocationsByName : ShoppingServiceSpec<object>
{
    public WhenPlaceOrderInvocationsByName() => When(_ => _.PlaceOrder(A<ShoppingCart>()));

    [Fact]
    public void ThenCreateOrderWasInvokedOnce()
    {
        Then<IOrderService>(nameof(IOrderService.CreateOrder), Once);
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService.CreateOrder was invoked once
            """);
    }

    [Fact]
    public void ThenWarningWasNotInvoked()
    {
        Then<ILogger>(nameof(ILogger.Warning), Never);
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then ILogger.Warning was not invoked
            """);
    }

    [Fact]
    public void ThenNamedInvocationsCompose()
    {
        Then<IOrderService>(nameof(IOrderService.CreateOrder), Once)
            .And<ILogger>(nameof(ILogger.Warning), Never);
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService.CreateOrder was invoked once
              and ILogger.Warning was not invoked
            """);
    }

    [Fact]
    public void ThenNamedInvocationComposesWithAggregate()
    {
        Then<IOrderService>(wasInvoked: Once)
            .And<IOrderService>(nameof(IOrderService.CreateOrder), AtMost(2));
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService was invoked once
              and IOrderService.CreateOrder was invoked AtMost(2)
            """);
    }

    [Fact]
    public void ThenTimesFactoryFormIsSupported()
        => Then<IOrderService>(nameof(IOrderService.CreateOrder), Once());

    [Fact]
    public void ThenNeverFails()
    {
        var ex = Xunit.Assert.Throws<XunitException>(
            () => Then<IOrderService>(nameof(IOrderService.CreateOrder), Never()));
        ex.Message.Is("Expected IOrderService.CreateOrder to be invoked never but was invoked 1 times");
    }
}

public class WhenCreateCartInvocationsByName : Spec<Subjects.ShoppingService, ShoppingCart>
{
    public WhenCreateCartInvocationsByName() => When(_ => _.CreateCart(An<int>()));

    [Fact]
    public void ThenCreateOrderWasNotInvoked()
    {
        Then<IOrderService>(nameof(IOrderService.CreateOrder), Never);
        Specification.Is(
            """
            When _.CreateCart(an int)
            Then IOrderService.CreateOrder was not invoked
            """);
    }

    [Fact]
    public void ThenWasInvokedOnceFailsWhenNeverCalled()
    {
        var ex = Xunit.Assert.Throws<XunitException>(
            () => Then<IOrderService>(nameof(IOrderService.CreateOrder), Once));
        ex.Message.Is("Expected IOrderService.CreateOrder to be invoked once but was invoked 0 times");
    }
}