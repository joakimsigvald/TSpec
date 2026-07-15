using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Pipeline;

public class AfterWhenBefore : Spec<MyStateService, int>
{
    [Fact]
    public void AfterIsExecutedBeforeWhen()
    {
        When(_ => ++_.Counter).Having(_ => _.Counter++).Then().Result.Is(2);
        Specification.Is(
            """
            When ++_.Counter
            After _.Counter++
            Then Result is 2
            """);
    }

    [Fact]
    public void FirstAfterIsExecutedAfterSecondAfterBeforeWhen()
    {
        When(_ => _.Counter *= 2)
            .Having(_ => _.Counter = 3)
            .Having(_ => _.Counter = 5)
            .Then().Result.Is(6);
        Specification.Is(
            """
            When _.Counter *= 2
            After _.Counter = 3
            After _.Counter = 5
            Then Result is 6
            """);
    }

    [Fact]
    public void GivenWhenExecutedTwice_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => ++_.Counter).When(_ => _.Counter *= 2));

    [Fact]
    public void GivenBeforeExecutedTwice_BothAreExecuted()
    {
        When(_ => _.Counter = 1).Until(_ => _.Counter = 3).Until(_ => _.Counter = 2)
            .Then().Result.Is(1);
        Specification.Is(
            """
            When _.Counter = 1
            Before _.Counter = 3
            Before _.Counter = 2
            Then Result is 1
            """);
    }

    [Fact]
    public void GivenSetupFail_ThenDontTearDown()
    {
        var ex = Xunit.Assert.Throws<SetupFailed>(() 
            => Given().A<MyModel>(m => throw new ApplicationException())
            .When(_ => A<MyModel>())
            .Until(_ => throw new InvalidOperationException("Unexpected exception"))
            .Then());
        ex.InnerException.Has().Type<ApplicationException>();
        Specification.Is(
            """
            Given a MyModel is throw new ApplicationException()
            Ex.InnerException has type ApplicationException
            """);
    }

    [Fact]
    public void GivenCallThenBeforeWhen_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() => Then().Throws<Exception>());

    [Fact]
    public void WhenBeforeGivenAfter()
    {
        When(_ => ++_.Counter).Until(_ => ++_.Counter).Using(1).Having(_ => _.Counter++).Then().Result.Is(2);
        Specification.Is(
            """
            Using 1
            When ++_.Counter
            After _.Counter++
            Before ++_.Counter
            Then Result is 2
            """);
    }
}

public class GivenTearDown : Spec<MyStateService, int>
{
    private int _theCounterAfterTest = -1;

    [Fact]
    public void BeforeIsExecutedAfterWhen()
    {
        When(_ => ++_.Counter).Until(_ => _theCounterAfterTest = --_.Counter).Then().Result.Is(1);
        Specification.Is(
            """
            When ++_.Counter
            Before _theCounterAfterTest = --_.Counter
            Then Result is 1
            """);
        Xunit.Assert.Equal(-1, _theCounterAfterTest); //Teardown is performed after executing the test method
    }
}