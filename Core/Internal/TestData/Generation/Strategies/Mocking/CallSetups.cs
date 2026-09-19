using System.Collections.Concurrent;
using System.Reflection;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// The answers a mock was set up with. The latest setup matching a call answers it; a child's own
/// setups come first, and a call they leave unanswered goes to the shared mock's.
/// </summary>
/// <remarks>
/// A call is set up with one answer: a function from the call's arguments to what it answers with,
/// or a throw. Reading the call and answering it are the same function, so a tap, an outcome and a
/// sequence step compose without an order to rely on. The answer produces a value of its own answer
/// type; where the call is awaited and that is the value inside the task, the task is made here.
/// </remarks>
internal sealed class CallSetups(CallSetups? shared)
{
    private readonly ConcurrentQueue<CallSetup> _setups = [];

    internal void Add(CallMatcher matcher, Type answerType, Func<IReadOnlyList<object>, object?> answer)
        => _setups.Enqueue(new(
            matcher, arguments => AsyncAnswer.Respond(matcher.ReturnType, answerType, () => answer(arguments!))));

    internal bool TryAnswer(MethodInfo method, object?[] arguments, out object? answer)
    {
        answer = null;
        if (LatestMatching(method, arguments) is not { } setup)
            return false;

        setup.Matcher.WriteOutArguments(arguments);
        answer = setup.Respond(arguments);
        return true;
    }

    private CallSetup? LatestMatching(MethodInfo method, object?[] arguments)
        => OwnLatestMatching(method, arguments) ?? shared?.LatestMatching(method, arguments);

    private CallSetup? OwnLatestMatching(MethodInfo method, object?[] arguments)
        => _setups.LastOrDefault(setup => setup.Matcher.Matches(method, arguments));

    private sealed record CallSetup(CallMatcher Matcher, Func<object?[], object?> Respond);
}
