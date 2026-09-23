using TSpec.Assert;
using static TSpec.Times;

namespace TSpec.Test.AutoMock;

public class WhenOneMockOfATypeIsVerified : Spec<RulePair, bool>
{
    public WhenOneMockOfATypeIsVerified()
        => When(_ => _.FirstPasses()).Using(new RulePair(A<IRule>(), ASecond<IRule>()));

    [Fact]
    public void ThenACallCountsOnTheMockThatReceivedIt()
        => Then(The<IRule>, _ => _.Passes(), Once).And(TheSecond<IRule>, _ => _.Passes(), Never);

    [Fact]
    public void ThenACallWithoutACountIsVerifiedAtLeastOnce() => Then(The<IRule>, _ => _.Passes());

    [Fact]
    public void ThenTheWholeMockCountsItsOwnCalls()
        => Then(The<IRule>, wasInvoked: Once).And(TheSecond<IRule>, wasInvoked: Never);

    [Fact]
    public void ThenAMethodByNameCountsItsOwnCalls()
        => Then(The<IRule>, nameof(IRule.Passes), Once).And(TheSecond<IRule>, nameof(IRule.Passes), Never);

    [Fact]
    public void ThenTheSpecificationNamesTheMock()
    {
        Then(TheSecond<IRule>, _ => _.Passes(), Never);
        Specification.Is(
            """
            Using new RulePair(a IRule, a second IRule)
            When FirstPasses()
            Then the second IRule.Passes() was not invoked
            """);
    }

    [Fact]
    public void GivenTheCallWasMadeOnlyOnAnother_ThenTheFailureSaysSo()
        => MessageOf(() => Then(TheSecond<IRule>, _ => _.Passes(), Once)).Is(
            """
            Expected the second IRule.Passes() to be invoked once but was never invoked
            the second IRule received no calls
            Passes() was invoked on other instances of IRule
            """);

    [Fact]
    public void GivenTheCallWasMadeOnNoMock_ThenTheFailureSaysNothingOfTheOthers()
        => MessageOf(() => Then(TheSecond<IRule>, _ => _.Weight(), Once)).Is(
            """
            Expected the second IRule.Weight() to be invoked once but was never invoked
            the second IRule received no calls
            """);

    [Fact]
    public void GivenTheMethodWasCalledOnlyOnAnother_ThenTheFailureSaysSo()
        => MessageOf(() => Then(TheSecond<IRule>, nameof(IRule.Passes), Once)).Is(
            """
            Expected the second IRule.Passes to be invoked once but was never invoked
            the second IRule received no calls
            Passes was invoked on other instances of IRule
            """);

    [Fact]
    public void GivenOnlyAnotherWasCalled_ThenTheFailureSaysSo()
        => MessageOf(() => Then(TheSecond<IRule>, wasInvoked: Once)).Is(
            """
            Expected the second IRule to be invoked once but was never invoked
            Other instances of IRule were invoked
            """);

    private static string MessageOf(Action verification)
        => WhenAMockIsNamedInAMessage.MessageOf(verification);
}

public class WhenATaggedMockIsVerified : Spec<RulePair, bool>
{
    private static readonly Tag<IRule> _primary = new();

    public WhenATaggedMockIsVerified()
        => When(_ => _.FirstPasses()).Using(new RulePair(The(_primary), A<IRule>()));

    [Fact]
    public void ThenACallCountsOnTheTaggedMock()
        => Then(_primary, _ => _.Passes(), Once).And(_primary, _ => _.Weight(), Never);

    [Fact]
    public void ThenTheWholeTaggedMockCountsItsOwnCalls()
        => Then(_primary, wasInvoked: Once).And(_primary, nameof(IRule.Passes), Once);
}
