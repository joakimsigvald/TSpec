using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// The calls a mock received, in order. A child's calls are recorded on the shared mock's log too,
/// which so holds every call to the type.
/// </summary>
internal sealed class CallLog(CallLog? shared)
{
    private static readonly ConditionalWeakTable<object, CallLog> _logs = [];

    private readonly ConcurrentQueue<MockInvocation> _calls = [];
    private readonly ConcurrentQueue<MockInvocation> _own = [];

    internal IReadOnlyList<MockInvocation> Counted => [.. _calls.Where(call => call.IsCounted)];

    internal void RecordsCallsOf(object instance) => _logs.Add(instance, this);

    internal void Record(MockInvocation call)
    {
        _own.Enqueue(call);
        Include(call);
    }

    internal int Count(LambdaExpression call) => Counting(call)(_calls);

    private void Include(MockInvocation call)
    {
        _calls.Enqueue(call);
        shared?.Include(call);
    }

    /// <summary>
    /// A count made ready to take over a mock's calls; every step of a chain is read now. The rest of a
    /// chain is counted on each mock its first step answered with, once, among the calls that mock
    /// received itself. A step taken while arranging still leads on to the mock it answered with.
    /// </summary>
    private static Func<IEnumerable<MockInvocation>, int> Counting(LambdaExpression call)
    {
        if (!CallChain.TrySplit(call, out var firstStep, out var rest))
        {
            var matcher = CallMatcher.For(call);
            return calls => calls.Count(called => called.IsCounted && matcher.Matches(called));
        }

        var step = CallMatcher.For(firstStep);
        var countRest = Counting(rest);
        return calls => calls
            .Where(step.Matches)
            .Select(reached => LogOf(reached.Answer))
            .OfType<CallLog>()
            .Distinct()
            .Sum(log => countRest(log._own));
    }

    private static CallLog? LogOf(object? answer)
        => AsyncAnswer.ValueOf(answer) is { } value && _logs.TryGetValue(value, out var log) ? log : null;
}
