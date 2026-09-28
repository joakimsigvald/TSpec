using System.Collections.Concurrent;
using TSpec.Internal.Specification;
using TSpec.Internal.Verification;
using Xunit.v3;

namespace TSpec.Internal.Pipelines;

internal interface ISharedRun
{
    void TearDown();
}

internal sealed record SharedRun<TSUT, TResult>(
    TestResult<TSUT, TResult> Outcome, Pipeline<TSUT, TResult> Maker, SpecificationContext Specification, object Test)
    : ISharedRun
{
    internal List<string> Facts { get; } = [];

    public void TearDown() => Maker.TearDownRun();
}

/// <summary>
/// One run per spec class, made by the first Fact that reads the outcome before anything else and
/// taken by the rest.
/// </summary>
internal static class SharedRuns
{
    private static readonly ConcurrentDictionary<Type, ISharedRun> _runs = new();

    /// <summary>
    /// Whether the pipeline belongs to the running test, that test is a Fact and sharing is not
    /// turned off. A spec built inside the test, such as one its act runs, has a pipeline of its own
    /// and never shares; each row of a Theory is a scenario of its own.
    /// </summary>
    internal static bool TryGetTest<TSUT, TResult>(Pipeline<TSUT, TResult> pipeline, out object test)
    {
        test = TestContext.Current.TestClassInstance!;
        return test is Spec<TSUT, TResult> spec
            && ReferenceEquals(spec.Pipeline, pipeline)
            && TestContext.Current.TestMethod is IXunitTestMethod { Method: var method }
            && !method.IsDefined(typeof(TheoryAttribute), inherit: true)
            && !IsTurnedOff;
    }

    /// xUnit runs the tests of a class one at a time, so the run is made once without a lock.
    internal static SharedRun<TSUT, TResult> GetOrRun<TSUT, TResult>(
        Type specClass, Func<SharedRun<TSUT, TResult>> run)
        => (SharedRun<TSUT, TResult>)_runs.GetOrAdd(specClass, _ => run());

    internal static void End(Type specClass)
    {
        if (_runs.TryRemove(specClass, out var run))
            run.TearDown();
    }

    private static bool IsTurnedOff => Environment.GetEnvironmentVariable("TSPEC_SHARING") is "off";
}
