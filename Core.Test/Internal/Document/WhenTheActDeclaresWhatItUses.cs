using TSpec.Assert;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// Which of a spec's two type arguments say anything is decided by the act, not by the declaration:
/// every <c>When</c> overload already knows whether it hands over the subject and whether it yields
/// a result, so the pipeline is told rather than left to infer it.
/// </summary>
public class WhenTheActDeclaresWhatItUses
{
    private sealed class Acted
    {
        internal int Id { get; set; }
        internal void Ping() { }
        internal Task PingAsync() => Task.CompletedTask;
    }

    private sealed class Probe : Spec<Acted, int>
    {
        internal (bool Subject, bool Result) Uses => (Pipeline.ActsOnSubject, Pipeline.YieldsResult);
    }

    private static (bool Subject, bool Result) Uses(Action<Probe> act)
    {
        var probe = new Probe();
        act(probe);
        return probe.Uses;
    }

    [Fact]
    public void GivenAnActOnTheSubjectReturningTheResult_ThenUseBoth()
        => Uses(spec => spec.When(_ => _.Id)).Is((true, true));

    [Fact]
    public void GivenAnActTakingNoSubject_ThenUseTheResultAlone()
        => Uses(spec => spec.When(() => 1)).Is((false, true));

    [Fact]
    public void GivenAnActYieldingNothing_ThenUseTheSubjectAlone()
        => Uses(spec => spec.When(_ => _.Ping())).Is((true, false));

    [Fact]
    public void GivenAnActTakingNothingAndYieldingNothing_ThenUseNeither()
        => Uses(spec => spec.When(() => { })).Is((false, false));

    /// <summary>A Task is how an act says it yields nothing; a Task&lt;T&gt; still yields a T.</summary>
    [Fact]
    public void GivenAnAsyncActYieldingNothing_ThenUseTheSubjectAlone()
        => Uses(spec => spec.When(_ => _.PingAsync())).Is((true, false));

    [Fact]
    public void GivenAnAsyncActYieldingAResult_ThenUseBoth()
        => Uses(spec => spec.When(_ => Task.FromResult(_.Id))).Is((true, true));
}
