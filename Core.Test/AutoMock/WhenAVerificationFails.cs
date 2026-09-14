using TSpec.Assert;
using TSpec.Internal.Specification;
using Xunit.Sdk;
using static TSpec.Times;

namespace TSpec.Test.AutoMock;

/// A failed verification lists the calls the mock received, in order, each written as a call.
public class WhenAVerificationFails : Spec<MemberKindsService, string>
{
    [Fact]
    public void GivenAMethodCall_ThenItsArgumentsAreListed()
        => MessageOf(() => When(_ => _.SumOf(1, 2)).Then<IMemberKinds>(wasInvoked: Never))
            .Is(Expected(
                """
                Expected IMemberKinds to be invoked never but was invoked once
                IMemberKinds received:
                  IMemberKinds.Sum([1, 2])
                """));

    [Fact]
    public void GivenAPropertyIsSetThenReadBack_ThenBothCallsAreListedInOrder()
        => MessageOf(() => When(_ => _.NameThenGet("x")).Then<IMemberKinds>(wasInvoked: Once))
            .Is(Expected(
                """
                Expected IMemberKinds to be invoked once but was invoked 2 times
                IMemberKinds received:
                  IMemberKinds.Name = "x"
                  IMemberKinds.Get("x")
                """));

    [Fact]
    public void GivenAPropertyIsRead_ThenItIsListedAsTheProperty()
        => MessageOf(() => When(_ => _.GetName()).Then<IMemberKinds>(wasInvoked: Never))
            .Is(Expected(
                """
                Expected IMemberKinds to be invoked never but was invoked once
                IMemberKinds received:
                  IMemberKinds.Name
                """));

    [Fact]
    public void GivenAGenericMethod_ThenItsTypeArgumentIsListed()
        => MessageOf(() => When(_ => _.EchoInt(5)).Then<IMemberKinds>(wasInvoked: Never))
            .Is(Expected(
                """
                Expected IMemberKinds to be invoked never but was invoked once
                IMemberKinds received:
                  IMemberKinds.Echo<int>(5)
                """));

    [Fact]
    public void GivenADelegate_ThenItsInvocationIsListedAsTheDelegate()
        => MessageOf(() => When(_ => _.Lookup(1)).Then<Func<int, string>>(wasInvoked: Never))
            .Is(Expected(
                """
                Expected Func<int, string> to be invoked never but was invoked once
                Func<int, string> received:
                  Func<int, string>(1)
                """));

    [Fact]
    public void GivenAnExpressionThatDoesNotMatch_ThenTheCallsMadeInsteadAreListed()
        => MessageOf(() => When(_ => _.GetByKey("k")).Then<IMemberKinds>(_ => _.Get("other")))
            .Is(Expected(
                """
                Expected IMemberKinds.Get("other") to be invoked at least once but was never invoked
                IMemberKinds received:
                  IMemberKinds.Get("k")
                """));

    private static string MessageOf(Action verification)
        => Xunit.Assert.Throws<XunitException>(verification).Message.NormalizeLineEndings();

    private static string Expected(string message) => message.NormalizeLineEndings();
}
