using TSpec.Assert;
using TSpec.Test.Pipeline;

[assembly: TSpec.ShareableThen(typeof(WhenSharingIsTurnedOff), nameof(WhenSharingIsTurnedOff.ThenOneMakesItsOwnRun))]
[assembly: TSpec.ShareableThen(typeof(WhenSharingIsTurnedOff), nameof(WhenSharingIsTurnedOff.ThenAnotherMakesItsOwnToo))]

namespace TSpec.Test.Pipeline;

[Collection(nameof(SharingTurnedOff))]
public class WhenSharingIsTurnedOff : Spec<int>
{
    private static int _runs;
    private static int _thens;

    public WhenSharingIsTurnedOff() => When(() => ++_runs);

    [Fact] public void ThenOneMakesItsOwnRun() => Result.Is(++_thens);

    [Fact] public void ThenAnotherMakesItsOwnToo() => Result.Is(++_thens);
}

/// The variable is process-wide: set while other tests run, it would stop them sharing.
[CollectionDefinition(nameof(SharingTurnedOff), DisableParallelization = true)]
public class SharingTurnedOff : ICollectionFixture<SharingTurnedOff.Variable>
{
    public sealed class Variable : IDisposable
    {
        public Variable() => Environment.SetEnvironmentVariable("TSPEC_SHARING", "off");

        public void Dispose() => Environment.SetEnvironmentVariable("TSPEC_SHARING", null);
    }
}
