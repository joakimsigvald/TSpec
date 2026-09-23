using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace TSpec.Internal.Mocking;

/// <summary>
/// What a mock, or every mock of a type, was set up with: the answers to its calls, and the chains
/// whose rest applies to the children it reaches. The latest setup matching a call answers it; a
/// mock's own setups come first, and a call they leave unanswered goes to the type's.
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
    private readonly List<ChainedSetup> _chains = [];

    internal void Add(LambdaExpression call, Type answerType, Func<IReadOnlyList<object>, object?> answer)
        => Prepare(call, answerType, answer)(this);

    /// A call answering with nothing, which a property read is not.
    internal void AddVoid(LambdaExpression call, Func<IReadOnlyList<object>, object?> answer)
    {
        CallReader.AssertIsNotARead(call);
        Add(call, typeof(void), answer);
    }

    /// <summary>
    /// A member no expression can name — a protected method or property. A name states no
    /// arguments, so every parameter takes whatever it is passed.
    /// </summary>
    internal void Add(MemberInfo member, Type answerType, Func<IReadOnlyList<object>, object?> answer)
        => Add(CallMatcher.For(member), answerType, answer);

    internal void Add(CallMatcher matcher, Type answerType, Func<IReadOnlyList<object>, object?> answer)
        => Add(matcher, answerType, (_, arguments) => answer(arguments));

    /// <summary>
    /// A setup made ready to apply. Every step of a chain is read now, as the setup is made, though
    /// the rest of it applies only to the children reached at an address its first step matches.
    /// </summary>
    private static Action<CallSetups> Prepare(
        LambdaExpression call, Type answerType, Func<IReadOnlyList<object>, object?> answer)
    {
        if (!CallChain.TrySplit(call, out var firstStep, out var rest))
        {
            var matcher = CallMatcher.For(call);
            return setups => setups.Add(matcher, answerType, answer);
        }

        var step = CallMatcher.For(firstStep);
        var childType = firstStep.Body.Type;
        var setUpChild = Prepare(rest, answerType, answer);
        return setups => setups.AddChain(step, childType, setUpChild);
    }

    /// A chain's first step answers with a child of the mock that received it, so each mock reaches its own.
    private void AddChain(CallMatcher firstStep, Type childType, Action<CallSetups> setUpChild)
    {
        lock (_chains)
            _chains.Add(new(firstStep, setUpChild));
        Add(firstStep, childType, (mock, arguments) => mock.ChildAt(firstStep.Method, childType, arguments).Instance);
    }

    private void Add(CallMatcher matcher, Type answerType, Func<MockHandle, IReadOnlyList<object>, object?> answer)
        => _setups.Enqueue(new(
            matcher,
            (mock, arguments) => AsyncAnswer.Respond(matcher.ReturnType, answerType, () => answer(mock, arguments!))));

    internal bool TryAnswer(MockHandle mock, MethodInfo method, object?[] arguments, out object? answer)
    {
        answer = null;
        if (LatestMatching(method, arguments) is not { } setup)
            return false;

        setup.Matcher.WriteOutArguments(arguments);
        answer = setup.Respond(mock, arguments);
        return true;
    }

    /// The rest of every chain whose first step matches, the type's first, so those made on the mock win.
    internal IEnumerable<Action<CallSetups>> ChainsFor(MethodInfo method, IReadOnlyList<object?> arguments)
        => (shared?.ChainsFor(method, arguments) ?? []).Concat(OwnChainsFor(method, arguments));

    private Action<CallSetups>[] OwnChainsFor(MethodInfo method, IReadOnlyList<object?> arguments)
    {
        lock (_chains)
            return [.. _chains.Where(chain => chain.FirstStep.Matches(method, arguments)).Select(chain => chain.SetUpChild)];
    }

    private CallSetup? LatestMatching(MethodInfo method, object?[] arguments)
        => OwnLatestMatching(method, arguments) ?? shared?.LatestMatching(method, arguments);

    private CallSetup? OwnLatestMatching(MethodInfo method, object?[] arguments)
        => _setups.LastOrDefault(setup => setup.Matcher.Matches(method, arguments));

    private sealed record CallSetup(CallMatcher Matcher, Func<MockHandle, object?[], object?> Respond);

    private sealed record ChainedSetup(CallMatcher FirstStep, Action<CallSetups> SetUpChild);
}
