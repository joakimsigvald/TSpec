using System.Linq.Expressions;
using System.Reflection;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// TSpec's hold on one mock: the type it stands in for, the instance handed to the subject, and the
/// calls that instance received. Castle makes the instance; every call it receives is logged, then
/// answered by the latest setup that matches it, or else by TSpec's defaults.
/// </summary>
internal sealed class MockHandle
{
    private readonly MockRegistry _mocks;
    private readonly CallLog _log;
    private readonly CallSetups _setups;
    private readonly MockChildren _children;
    private readonly KeptAnswers _answers = new();
    private readonly object? _instance;

    internal MockHandle(Type mockedType, MockRegistry mocks, MockHandle? shared = null)
    {
        MockedType = mockedType;
        _mocks = mocks;
        _log = new(shared?._log);
        _setups = new(shared?._setups);
        _children = new(mocks, shared?._children);
        _instance = MockInstance.Create(mockedType, Receive, () => mocks.Defaults.GetConstructorArguments(mockedType));
        _log.RecordsCallsOf(_instance);
    }

    internal Type MockedType { get; }

    internal object Instance => _instance!;

    internal IReadOnlyList<MockInvocation> CountedInvocations => _log.Counted;

    internal void Answer<TService>(Expression<Action<TService>> call, Func<IReadOnlyList<object>, object?> answer)
        where TService : class
    {
        CallReader.AssertIsNotARead(call);
        Prepare(call, typeof(void), answer)(this);
    }

    internal void Answer<TService, TResult>(
        Expression<Func<TService, TResult>> call, Type answerType, Func<IReadOnlyList<object>, object?> answer)
        where TService : class
        => Prepare(call, answerType, answer)(this);

    /// <summary>
    /// A member no expression can name — a protected method or property. A name states no
    /// arguments, so every parameter takes whatever it is passed.
    /// </summary>
    internal void Answer<TService>(MemberInfo member, Type answerType, Func<IReadOnlyList<object>, object?> answer)
        where TService : class
        => _setups.Add(CallMatcher.For(member), answerType, answer);

    internal int CountCalls(LambdaExpression call) => _log.Count(call);

    /// <summary>
    /// A setup made ready to apply to a mock. Every step of a chain is read now, as the setup is made,
    /// though the rest of it applies only to the children reached at an address its first step matches.
    /// </summary>
    private static Action<MockHandle> Prepare(
        LambdaExpression call, Type answerType, Func<IReadOnlyList<object>, object?> answer)
    {
        if (!CallChain.TrySplit(call, out var firstStep, out var rest))
        {
            var matcher = CallMatcher.For(call);
            return mock => mock._setups.Add(matcher, answerType, answer);
        }

        var step = CallMatcher.For(firstStep);
        var childType = firstStep.Body.Type;
        var setUpChild = Prepare(rest, answerType, answer);
        return mock => mock.SetUpChain(step, childType, setUpChild);
    }

    private void SetUpChain(CallMatcher firstStep, Type childType, Action<MockHandle> setUpChild)
    {
        _children.Add(firstStep, setUpChild);
        _setups.Add(firstStep, childType, arguments => _children.At(firstStep.Method, childType, arguments).Instance);
    }

    /// A call is logged before it is answered, so whatever answers it may read the log; what it answered
    /// with is logged after.
    private object? Receive(MethodInfo method, object?[] arguments)
    {
        _mocks.Guard.Check(MockedType, method, arguments);
        var invocation = new MockInvocation(method, [.. arguments], _mocks.Phase);
        _log.Record(invocation);
        invocation.Answer = Respond(method, arguments);
        return invocation.Answer;
    }

    private object? Respond(MethodInfo method, object?[] arguments)
        => PropertyAccess.IsSet(method)
            ? RespondToSet(method, arguments)
            : RespondToCall(method, arguments);

    /// A set is answered as any call is, so a setup on it still throws or taps; what it keeps is
    /// written only once that returned, since a set that threw never happened.
    private object? RespondToSet(MethodInfo setter, object?[] arguments)
    {
        var answer = RespondToCall(setter, arguments);
        _answers.Write(CallAddress.Of(setter, arguments), arguments[^1]);
        return answer;
    }

    /// <summary>
    /// The latest setup matching a call answers it; a call no setup matches is answered once per
    /// address and answers the same from then on, except one made while the instance is still being
    /// constructed, which has no mock to be answered for yet and gets its type's default.
    /// </summary>
    private object? RespondToCall(MethodInfo method, object?[] arguments)
    {
        var returnType = method.ReturnType;
        if (_setups.TryAnswer(method, arguments, out var answer))
            return answer ?? DefaultOf(returnType);
        if (returnType == typeof(void))
            return null;
        if (_instance is null)
            return DefaultOf(returnType);
        return KeptAnswer(CallAddress.Of(method, arguments), returnType);
    }

    private object? KeptAnswer(CallAddress address, Type returnType)
    {
        if (_answers.TryRead(address, out var kept))
            return kept;

        var answer = _mocks.Defaults.GetDefaultValue(returnType, this, address) ?? DefaultOf(returnType);
        _answers.Write(address, answer);
        return answer;
    }

    /// A call answered with the mock of its return type is answered with one mock per address
    /// instead, as a chained setup is, so what is reached through other arguments is counted apart.
    internal object? PerAddress(Type type, object? value, CallAddress address)
        => _mocks.IsTypeMock(type, value)
            ? _children.At(address.Member, type, address.Arguments).Instance
            : value;

    private static object? DefaultOf(Type type)
        => type.IsValueType && type != typeof(void) ? Activator.CreateInstance(type) : null;
}
