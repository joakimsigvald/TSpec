using TSpec.Assert;

namespace TSpec.Test.Tests.Delay;

public abstract class WhenGetStateAfterSetStateWithHavingDelay : Spec<DelayedState, int>
{
    private static readonly Tag<int> _delay = new(), _state = new(), _wait = new();

    protected WhenGetStateAfterSetStateWithHavingDelay()
        => Using(_delay)
        .When(_ => _.State)
        .Having(_ => _.SetState(The(_state)), () => The(_wait));

    public class GivenZeroDelay : WhenGetStateAfterSetStateWithHavingDelay
    {
        public GivenZeroDelay() => Given(_delay).Is(0);
        [Fact] public void ThenGetNewState() => Result.Is(The(_state));
    }

    public class GivenWaitShorterThanDelay : WhenGetStateAfterSetStateWithHavingDelay
    {
        public GivenWaitShorterThanDelay() => Given(_delay).Is(200).And(_wait).Is(100);
        [Fact] public void ThenGetInitialState() => Result.Is(0);
    }

    public class GivenWaitLongerThanDelay : WhenGetStateAfterSetStateWithHavingDelay
    {
        public GivenWaitLongerThanDelay() => Given(_delay).Is(100).And(_wait).Is(200);
        [Fact]
        public void ThenGetNewState()
        {
            Result.Is(The(_state));
            Specification.Is(
                """
                Using Delay
                Given Wait is 200
                  and Delay is 100
                When State
                Having waited the Wait ms
                  after SetState(the State)
                Then Result is the State
                """);
        }
    }
}

public abstract class WhenGetStateAfterSetStateWithAsyncTaskDelay : Spec<DelayedState, int>
{
    private static readonly Tag<int> _delay = new(), _state = new(), _wait = new();

    protected WhenGetStateAfterSetStateWithAsyncTaskDelay()
        => Using(() => The(_delay), For.Subject)
        .When(_ => _.State)
        .Having(async Task (_) =>
        {
            _.SetState(The(_state));
            await Task.Delay(The(_wait));
        });

    public class GivenZeroDelay : WhenGetStateAfterSetStateWithAsyncTaskDelay
    {
        public GivenZeroDelay() => Given(_delay).Is(0);
        [Fact] public void ThenGetNewState() => Result.Is(The(_state));
    }

    public class GivenWaitShorterThanDelay : WhenGetStateAfterSetStateWithAsyncTaskDelay
    {
        public GivenWaitShorterThanDelay() => Given(_delay).Is(200).And(_wait).Is(100);
        [Fact] public void ThenGetInitialState() => Result.Is(0);
    }

    public class GivenWaitLongerThanDelay : WhenGetStateAfterSetStateWithAsyncTaskDelay
    {
        public GivenWaitLongerThanDelay() => Given(_delay).Is(100).And(_wait).Is(200);
        [Fact] public void ThenGetNewState() => Result.Is(The(_state));
    }
}