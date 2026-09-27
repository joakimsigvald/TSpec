using System.Collections.Concurrent;
using System.Reflection;
using TSpec.Internal.Specification;
using TSpec.Internal.Verification;
using Xunit.v3;

namespace TSpec.Internal.Pipelines;

internal sealed record SharedRun<TSUT, TResult>(
    TestResult<TSUT, TResult> Outcome, SpecificationContext Specification);

/// <summary>
/// The runs shared by the test methods the source generator listed as only asserting on the result:
/// one per spec class, made by the first of them to ask and taken by the rest.
/// </summary>
internal static class SharedRuns
{
    private static readonly ConcurrentDictionary<Assembly, HashSet<(Type, string)>> _shareableThens = new();
    private static readonly ConcurrentDictionary<Type, object> _runs = new();
    private static readonly ConcurrentDictionary<Type, object> _locks = new();

    /// <summary>
    /// Whether the pipeline belongs to the running test and that test is listed as shareable. A spec
    /// built inside the test, such as one its act runs, has a pipeline of its own and never shares.
    /// </summary>
    internal static bool TryGetSpecClass<TSUT, TResult>(Pipeline<TSUT, TResult> pipeline, out Type specClass)
    {
        var instance = TestContext.Current.TestClassInstance;
        specClass = instance?.GetType()!;
        return instance is Spec<TSUT, TResult> spec
            && ReferenceEquals(spec.Pipeline, pipeline)
            && TestContext.Current.TestMethod is IXunitTestMethod { Method: var method }
            && IsShareable(method);
    }

    /// Holding the lock while the run is made lets a test running in parallel wait for it instead of making its own.
    internal static TRun GetOrRun<TRun>(Type specClass, Func<TRun> run) where TRun : class
    {
        lock (_locks.GetOrAdd(specClass, _ => new()))
        {
            if (_runs.TryGetValue(specClass, out var kept))
                return (TRun)kept;

            var made = run();
            _runs[specClass] = made;
            return made;
        }
    }

    private static bool IsShareable(MethodInfo method)
        => ShareableThensIn(method.DeclaringType!.Assembly).Contains((method.DeclaringType, method.Name));

    private static HashSet<(Type, string)> ShareableThensIn(Assembly assembly)
        => _shareableThens.GetOrAdd(assembly, _ => [.. assembly
            .GetCustomAttributes<ShareableThenAttribute>()
            .Select(then => (then.SpecClass, then.Method))]);
}
