using static TSpec.Times;
using TSpec.Assert;
using TSpec.Internal.Specification;
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
            When PlaceOrder(a ShoppingCart)
            Then IOrderService was invoked
            """);
    }

    [Fact]
    public void ThenOrderServiceWasInvokedOnce()
    {
        Then<IOrderService>(wasInvoked: Once);
        Specification.Is(
            """
            When PlaceOrder(a ShoppingCart)
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
            When PlaceOrder(a ShoppingCart)
            Then IOrderService was invoked Exactly(1)
              and IOrderService was invoked AtMost(2)
            """);
    }

    [Fact]
    public void ThenOrderServiceWasInvokedAtMostOnceAndBetween()
    {
        Then<IOrderService>(wasInvoked: AtMostOnce)
            .And<IOrderService>(wasInvoked: Between(1, 2));
        Specification.Is(
            """
            When PlaceOrder(a ShoppingCart)
            Then IOrderService was invoked AtMostOnce
              and IOrderService was invoked Between(1, 2)
            """);
    }

    [Fact]
    public void ThenQualifiedCountReadsAsTheBareOne()
    {
        Then<IOrderService>(wasInvoked: Exactly(1));
        Specification.Is(
            """
            When PlaceOrder(a ShoppingCart)
            Then IOrderService was invoked Exactly(1)
            """);
    }

    [Theory]
    [InlineData(-1, 1, "An invocation count cannot be negative, but -1 was given")]
    [InlineData(2, 1, "No invocation count is at least 2 and at most 1")]
    public void GivenBoundsNoCountCanMeet_ThenThrowSetupFailed(int from, int to, string error)
        => Xunit.Assert.Throws<SetupFailed>(() => Between(from, to)).Message.Is(error);

    [Fact]
    public void ThenWasInvokedComposesWithSpecificVerification()
    {
        Then<IOrderService>(wasInvoked: Once)
            .And<IOrderService>(_ => _.CreateOrder(The<ShoppingCart>()));
        Specification.Is(
            """
            When PlaceOrder(a ShoppingCart)
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
            When PlaceOrder(a ShoppingCart)
            Then IOrderService.CreateOrder(the ShoppingCart)
              and IOrderService was invoked once
            """);
    }

    [Fact]
    public void ThenWasInvokedNeverFails()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => Then<IOrderService>(wasInvoked: Never));
        ex.Message.NormalizeLineEndings().Is(
            $"""
            Expected IOrderService to be invoked never but was invoked once
            IOrderService received:
              IOrderService.CreateOrder({The<ShoppingCart>()})
            """.NormalizeLineEndings());
    }

    [Fact]
    public void ThenWasInvokedAtLeastTwiceFails()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => Then<IOrderService>(wasInvoked: AtLeast(2)));
        ex.Message.NormalizeLineEndings().Is(
            $"""
            Expected IOrderService to be invoked AtLeast(2) but was invoked once
            IOrderService received:
              IOrderService.CreateOrder({The<ShoppingCart>()})
            """.NormalizeLineEndings());
    }
}
