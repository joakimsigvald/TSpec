using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// <summary>
/// An async lambda written without an explicit return type must bind to the Task overloads.
/// These are compile-time tests as much as run-time ones: when the Task and ValueTask overloads
/// are ambiguous, the build fails rather than the test.
/// </summary>
public class WhenAsyncLambdaActsOnSubject : Spec<MyStateService, int>
{
    public WhenAsyncLambdaActsOnSubject()
        => When(async _ => { await Task.Yield(); _.Counter = 3; });

    [Fact]
    public void ThenTheAwaitedWorkHasCompleted() => Then().SubjectUnderTest.Counter.Is(3);
}

public class WhenAsyncLambdaActsWithoutSubject : Spec<MyStateService, int>
{
    private int _counter;

    public WhenAsyncLambdaActsWithoutSubject()
        => When(async () => { await Task.Yield(); _counter = 3; });

    [Fact]
    public void ThenTheAwaitedWorkHasCompleted()
    {
        Then();
        _counter.Is(3);
    }
}

public class WhenAsyncLambdaReturnsFromSubject : Spec<MyStateService, int>
{
    public WhenAsyncLambdaReturnsFromSubject()
        => When(async _ => { await Task.Yield(); return _.Counter + 3; });

    [Fact]
    public void ThenTheAwaitedResultIsReturned() => Then().Result.Is(3);
}

public class WhenAsyncLambdaReturnsWithoutSubject : Spec<MyStateService, int>
{
    public WhenAsyncLambdaReturnsWithoutSubject()
        => When(async () => { await Task.Yield(); return 3; });

    [Fact]
    public void ThenTheAwaitedResultIsReturned() => Then().Result.Is(3);
}

public class WhenAsyncLambdaReturnsNull : Spec<MyStateService, string?>
{
    public WhenAsyncLambdaReturnsNull()
        => When(async _ => { await Task.Yield(); return null; });

    [Fact]
    public void ThenTheResultIsNull() => Then().Result.Is(null);
}

public class WhenAsyncLambdaIsGivenAfterAnotherStep : Spec<MyStateService, int>
{
    public WhenAsyncLambdaIsGivenAfterAnotherStep()
        => Using(0).When(async _ => { await Task.Yield(); return _.Counter + 3; });

    [Fact]
    public void ThenTheAwaitedResultIsReturned() => Then().Result.Is(3);
}

public class HavingAsyncLambda : Spec<MyStateService, int>
{
    public HavingAsyncLambda()
        => When(_ => _.Counter * 2)
        .Having(async _ => { await Task.Yield(); _.Counter = 3; });

    [Fact]
    public void ThenTheSetupCompletedBeforeTheAct() => Then().Result.Is(6);
}

public class HavingAsyncLambdaWithDelay : Spec<MyStateService, int>
{
    public HavingAsyncLambdaWithDelay()
        => When(_ => _.Counter * 2)
        .Having(async _ => { await Task.Yield(); _.Counter = 3; }, () => 1);

    [Fact]
    public void ThenTheSetupCompletedBeforeTheAct() => Then().Result.Is(6);
}

/// <summary>
/// Stating ValueTask explicitly is the escape hatch from the Task overloads now outranking them.
/// </summary>
public class WhenAsyncLambdaStatesValueTask : Spec<MyStateService, int>
{
    public WhenAsyncLambdaStatesValueTask()
        => When(async ValueTask<int> (_) => { await Task.Yield(); return _.Counter + 3; })
        .Having(async ValueTask (_) => { await Task.Yield(); _.Counter = 1; });

    [Fact]
    public void ThenTheAwaitedResultIsReturned() => Then().Result.Is(4);
}

public class UntilAsyncLambda : Spec<MyStateService, int>
{
    private int _counterAfterTest = -1;

    public UntilAsyncLambda()
        => When(_ => ++_.Counter)
        .Until(async _ => { await Task.Yield(); _counterAfterTest = --_.Counter; });

    [Fact]
    public void ThenTheTearDownRunsAfterTheTestMethod()
    {
        Then().Result.Is(1);
        _counterAfterTest.Is(-1);
    }
}
