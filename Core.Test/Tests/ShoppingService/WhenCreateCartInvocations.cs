using static Moq.Times;
using TSpec.Assert;
using TSpec.Test.Subjects;
using Xunit.Sdk;

namespace TSpec.Test.Tests.ShoppingService;

public class WhenCreateCartInvocations : Spec<Subjects.ShoppingService, ShoppingCart>
{
    public WhenCreateCartInvocations() => When(_ => _.CreateCart(An<int>()));

    [Fact]
    public void ThenOrderServiceWasNotInvoked()
    {
        Then<IOrderService>(wasInvoked: Never);
        Specification.Is(
            """
            When CreateCart(an int)
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
