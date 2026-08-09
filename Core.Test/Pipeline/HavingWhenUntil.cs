using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Pipeline;

public sealed class CounterSpec : Spec<MyStateService, int> { }

public class HavingWhenUntil : Spec<CounterSpec, int>
{
    [Fact]
    public void HavingIsExecutedBeforeWhen()
    {
        When(_ => _.When(s => ++s.Counter).Having(s => s.Counter++).Then().Result).Then().Result.Is(2);
        Then().SubjectUnderTest.Specification.Is(
            """
            When ++Counter
            Having Counter++
            Then
            """);
    }

    [Fact]
    public void FirstHavingIsExecutedAfterSecondHavingBeforeWhen()
    {
        When(_ => _.When(s => s.Counter *= 2)
            .Having(s => s.Counter = 3)
            .Having(s => s.Counter = 5)
            .Then().Result).Then().Result.Is(6);
        Then().SubjectUnderTest.Specification.Is(
            """
            When Counter *= 2
            Having Counter = 3
              after Counter = 5
            Then
            """);
    }

    /// <summary>
    /// Declaring the act twice throws from the fluent call itself, before the inner pipeline runs, so
    /// nothing marks the failure as a nested one and the enclosing pipeline cannot tell it from its
    /// own. The limitation is deliberate — see <c>WhenTheActRunsASpec</c>.
    /// </summary>
    [Fact]
    public void GivenWhenExecutedTwice_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => _.When(s => ++s.Counter).When(s => s.Counter *= 2).Then().Result).Then());

    [Fact]
    public void GivenUntilExecutedTwice_BothAreExecuted()
    {
        When(_ => _.When(s => s.Counter = 1)
            .Until(s => s.Counter = 3)
            .Until(s => s.Counter = 2)
            .Then().Result).Then().Result.Is(1);
        Then().SubjectUnderTest.Specification.Is(
            """
            When Counter = 1
            Until Counter = 3
              before Counter = 2
            Then
            """);
    }

    [Fact]
    public void GivenSetupFail_ThenDontTearDown()
        => When(_ => _.Given().A<MyModel>(m => throw new ApplicationException())
                .When(s => s.Counter)
                .Until(void (s) => throw new InvalidOperationException("Unexpected exception"))
                .Then().Result)
            .Then().Throws<SetupFailed>().that.InnerException.Is().A<ApplicationException>();

    [Fact]
    public void GivenCallThenBeforeWhen_ThenThrowSetupFailed()
        => When(_ => _.Then().Result).Then().Throws<SetupFailed>();

    [Fact]
    public void GivenStepsDeclaredOutOfOrder_ThenSpecifyThemInPipelineOrder()
    {
        When(_ => _.When(s => ++s.Counter)
            .Until(s => ++s.Counter)
            .Using(1)
            .Having(s => s.Counter++)
            .Then().Result).Then().Result.Is(2);
        Then().SubjectUnderTest.Specification.Is(
            """
            Using 1
            When ++Counter
            Having Counter++
            Until ++Counter
            Then
            """);
    }
}

public class GivenTearDown : Spec<CounterSpec, int>
{
    private int _theCounterAfterTest = -1;

    [Fact]
    public void UntilIsExecutedAfterWhen()
    {
        When(_ => _.When(s => ++s.Counter)
            .Until(s => _theCounterAfterTest = --s.Counter)
            .Then().Result).Then().Result.Is(1);
        Then().SubjectUnderTest.Specification.Is(
            """
            When ++Counter
            Until _theCounterAfterTest = --Counter
            Then
            """);
        _theCounterAfterTest.Is(-1); //Teardown is performed after executing the test method
    }
}
