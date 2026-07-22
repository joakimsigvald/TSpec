using static Moq.Times;
using TSpec.Assert;
using TSpec.Test.Subjects;

namespace TSpec.Test.Tests.ShoppingService;

// Coverage for the deprecated .WasInvoked continuation, retained for back-compat.
#pragma warning disable CS0618
public class WhenWasInvokedDeprecated : ShoppingServiceSpec<object>
{
    public WhenWasInvokedDeprecated() => When(_ => _.PlaceOrder(A<ShoppingCart>()));

    [Fact]
    public void ThenDeprecatedWasInvokedStillWorks()
    {
        Then<IOrderService>().WasInvoked(Once);
        Specification.Is(
            """
            When _.PlaceOrder(a ShoppingCart)
            Then IOrderService was invoked once
            """);
    }

    [Fact]
    public void ThenDeprecatedAndWasInvokedStillWorks()
        => Then<IOrderService>(_ => _.CreateOrder(The<ShoppingCart>()))
            .And<IOrderService>().WasInvoked(Once);
}
#pragma warning restore CS0618
