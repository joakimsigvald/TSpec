using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Pipeline;

public class HavingWhenUntil : Spec<MyStateService, int>
{
    [Fact]
    public void HavingIsExecutedBeforeWhen()
    {
        When(_ => ++_.Counter).Having(_ => _.Counter++).Then().Result.Is(2);
        Specification.Is(
            """
            When ++_.Counter
            Having _.Counter++
            Then Result is 2
            """);
    }

    [Fact]
    public void FirstHavingIsExecutedAfterSecondHavingBeforeWhen()
    {
        When(_ => _.Counter *= 2)
            .Having(_ => _.Counter = 3)
            .Having(_ => _.Counter = 5)
            .Then().Result.Is(6);
        Specification.Is(
            """
            When _.Counter *= 2
            Having _.Counter = 3
              and _.Counter = 5
            Then Result is 6
            """);
    }

    [Fact]
    public void GivenWhenExecutedTwice_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => ++_.Counter).When(_ => _.Counter *= 2));

    [Fact]
    public void GivenUntilExecutedTwice_BothAreExecuted()
    {
        When(_ => _.Counter = 1).Until(_ => _.Counter = 3).Until(_ => _.Counter = 2)
            .Then().Result.Is(1);
        Specification.Is(
            """
            When _.Counter = 1
            Until _.Counter = 3
              and _.Counter = 2
            Then Result is 1
            """);
    }

    [Fact]
    public void GivenSetupFail_ThenDontTearDown()
    {
        var ex = Xunit.Assert.Throws<SetupFailed>(() 
            => Given().A<MyModel>(m => throw new ApplicationException())
            .When(_ => A<MyModel>())
            .Until(void (_) => throw new InvalidOperationException("Unexpected exception"))
            .Then());
        ex.InnerException.Is().A<ApplicationException>();
        Specification.Is(
            """
            Given a MyModel is throw new ApplicationException()
            Ex.InnerException is a ApplicationException
            """);
    }

    [Fact]
    public void GivenCallThenBeforeWhen_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() => Then().Throws<Exception>());

    [Fact]
    public void GivenStepsDeclaredOutOfOrder_ThenSpecifyThemInPipelineOrder()
    {
        When(_ => ++_.Counter).Until(_ => ++_.Counter).Using(1).Having(_ => _.Counter++).Then().Result.Is(2);
        Specification.Is(
            """
            Using 1
            When ++_.Counter
            Having _.Counter++
            Until ++_.Counter
            Then Result is 2
            """);
    }
}

public class GivenTearDown : Spec<MyStateService, int>
{
    private int _theCounterAfterTest = -1;

    [Fact]
    public void UntilIsExecutedAfterWhen()
    {
        When(_ => ++_.Counter).Until(_ => _theCounterAfterTest = --_.Counter).Then().Result.Is(1);
        Specification.Is(
            """
            When ++_.Counter
            Until _theCounterAfterTest = --_.Counter
            Then Result is 1
            """);
        Xunit.Assert.Equal(-1, _theCounterAfterTest); //Teardown is performed after executing the test method
    }
}