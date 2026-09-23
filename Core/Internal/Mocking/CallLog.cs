using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Mocking;

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

    internal int Count(LambdaExpression call, string callExpr)
        => Counting(call, new(call.Parameters[0].Type, callExpr))(_calls);

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
    private static Func<IEnumerable<MockInvocation>, int> Counting(LambdaExpression call, Verification verification)
    {
        if (!CallChain.TrySplit(call, out var firstStep, out var rest))
        {
            var matcher = CallMatcher.For(call);
            return calls => calls.Count(called => called.IsCounted && matcher.Matches(called));
        }

        var step = CallMatcher.For(firstStep);
        var countRest = Counting(rest, verification);
        return calls => LogsReachedBy(step, calls, verification).Distinct().Sum(log => countRest(log._own));
    }

    /// A step answering with anything but a mock recorded nothing, so what was called through it is
    /// refused rather than counted as none, which it would be whatever the subject did.
    private static IEnumerable<CallLog> LogsReachedBy(
        CallMatcher step, IEnumerable<MockInvocation> calls, Verification verification)
    {
        foreach (var reached in calls.Where(step.Matches))
            yield return LogOf(reached.Answer) ?? throw verification.Unverifiable(reached);
    }

    /// The verification being counted, as the test wrote it, so a refusal can name the setup to write.
    private sealed record Verification(Type Service, string Expression)
    {
        internal SetupFailed Unverifiable(MockInvocation step)
            => new($"{ReceivedCalls.Describe(step.Method.DeclaringType!.Alias(), step)} answers with "
                + $"{Answered(step.Answer)}, which records no calls. "
                + $"Set it up to verify through it: Given<{Service.Alias()}>().That({Expression})");

        private static string Answered(object? answer)
            => (AsyncAnswer.ValueOf(answer) ?? answer) is { } value ? $"a real {value.GetType().Alias()}" : "null";
    }

    private static CallLog? LogOf(object? answer)
        => AsyncAnswer.ValueOf(answer) is { } value && _logs.TryGetValue(value, out var log) ? log : null;
}
