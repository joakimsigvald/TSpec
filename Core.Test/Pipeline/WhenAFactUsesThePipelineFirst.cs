using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// Each run gets a number of its own, so the Facts can tell a shared run from their own whatever
/// order they run in.
public class WhenAFactUsesThePipelineFirst : Spec<int>
{
    private static int _runs;
    private static int? _sharedRun;
    private static readonly List<int> _ownRuns = [];

    public WhenAFactUsesThePipelineFirst() => When(() => ++_runs);

    [Fact] public void ThenOneShares() => ThenItTookTheSharedRun();

    [Fact] public void ThenAnotherShares() => ThenItTookTheSharedRun();

    [Fact]
    public void GivenItSetsUp_ThenItRunsAlone()
    {
        Given().A(5);
        ThenItMadeItsOwnRun();
    }

    [Fact]
    public void GivenItMentionsAValue_ThenItRunsAlone()
    {
        _ = The<string>();
        ThenItMadeItsOwnRun();
    }

    private void ThenItTookTheSharedRun()
    {
        Result.Is(_sharedRun ??= Result);
        _ownRuns.Does().not.Contain(Result);
    }

    private void ThenItMadeItsOwnRun()
    {
        Result.Is().Not(_sharedRun ?? 0);
        _ownRuns.Add(Result);
    }
}

/// Each row of a Theory is a scenario of its own.
public class WhenTheoryRowsRun : Spec<int>
{
    private static int _runs;
    private static readonly List<int> _seen = [];

    public WhenTheoryRowsRun() => When(() => ++_runs);

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ThenEachRowRunsAlone(int _)
    {
        _seen.Does().not.Contain(Result);
        _seen.Add(Result);
    }
}
