using static TSpec.Times;
using TSpec.Assert;
using TSpec.Internal.Specification;
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
            When PlaceOrder(a ShoppingCart)
            Then IOrderService.CreateOrder was invoked once
            """);
    }

    [Fact]
    public void ThenWarningWasNotInvoked()
    {
        Then<ILogger>(nameof(ILogger.Warning), Never);
        Specification.Is(
            """
            When PlaceOrder(a ShoppingCart)
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
            When PlaceOrder(a ShoppingCart)
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
            When PlaceOrder(a ShoppingCart)
            Then IOrderService was invoked once
              and IOrderService.CreateOrder was invoked AtMost(2)
            """);
    }

    [Fact]
    public void ThenQualifiedCountIsSupported()
        => Then<IOrderService>(nameof(IOrderService.CreateOrder), Times.Once);

    [Fact]
    public void ThenNeverFails()
    {
        var ex = Xunit.Assert.Throws<XunitException>(
            () => Then<IOrderService>(nameof(IOrderService.CreateOrder), Never));
        ex.Message.NormalizeLineEndings().Is(
            $"""
            Expected IOrderService.CreateOrder to be invoked never but was invoked once
            IOrderService received:
              IOrderService.CreateOrder({The<ShoppingCart>()})
            """.NormalizeLineEndings());
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
            When CreateCart(an int)
            Then IOrderService.CreateOrder was not invoked
            """);
    }

    [Fact]
    public void ThenWasInvokedOnceFailsWhenNeverCalled()
    {
        var ex = Xunit.Assert.Throws<XunitException>(
            () => Then<IOrderService>(nameof(IOrderService.CreateOrder), Once));
        ex.Message.NormalizeLineEndings().Is(
            """
            Expected IOrderService.CreateOrder to be invoked once but was never invoked
            IOrderService received no calls
            """.NormalizeLineEndings());
    }
}