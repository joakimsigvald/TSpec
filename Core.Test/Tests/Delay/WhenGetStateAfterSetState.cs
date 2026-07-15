using TSpec.Assert;

namespace TSpec.Test.Tests.Delay;

public abstract class WhenGetStateAfterSetStateWithAfterDelay : Spec<DelayedState, int>
{
    private static readonly Tag<int> _delay = new(), _state = new(), _wait = new();

    protected WhenGetStateAfterSetStateWithAfterDelay()
        => Using(_delay)
        .When(_ => _.State)
        .Having(_ => _.SetState(The(_state)), () => The(_wait));

    public class GivenZeroDelay : WhenGetStateAfterSetStateWithAfterDelay
    {
        public GivenZeroDelay() => Given(_delay).Is(0);
        [Fact] public void ThenGetNewState() => Result.Is(The(_state));
    }

    public class GivenWaitShorterThanDelay : WhenGetStateAfterSetStateWithAfterDelay
    {
        public GivenWaitShorterThanDelay() => Given(_delay).Is(200).And(_wait).Is(100);
        [Fact] public void ThenGetInitialState() => Result.Is(0);
    }

    public class GivenWaitLongerThanDelay : WhenGetStateAfterSetStateWithAfterDelay
    {
        public GivenWaitLongerThanDelay() => Given(_delay).Is(100).And(_wait).Is(200);
        [Fact]
        public void ThenGetNewState()
        {
            Result.Is(The(_state));
            Specification.Is(
                """
                Using _delay
                Given _wait is 200
                  and _delay is 100
                When _.State
                After wait () => The(_wait) ms
                After _.SetState(the _state)
                Then Result is the _state
                """);
        }
    }
}

public abstract class WhenGetStateAfterSetStateWithAsyncTaskDelay : Spec<DelayedState, int>
{
    private static readonly Tag<int> _delay = new(nameof(_delay)), _state = new(nameof(_state)), _wait = new(nameof(_wait));

    protected WhenGetStateAfterSetStateWithAsyncTaskDelay()
        => Using(() => The(_delay), For.Subject)
        .When(_ => _.State)
        .Having(async _ =>
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