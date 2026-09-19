using Castle.DynamicProxy;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using TSpec.Internal.Specification;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// TSpec's hold on one mock: the type it stands in for, the instance handed to the subject, and the
/// calls that instance received. Castle makes the instance; every call it receives is logged, then
/// answered by the latest setup that matches it, or else by TSpec's defaults.
/// </summary>
/// <remarks>
/// A call is set up with one answer: a function from the call's arguments to what it answers with,
/// or a throw. Reading the call and answering it are the same function, so a tap, an outcome and a
/// sequence step compose without an order to rely on. The answer produces a value of its own answer
/// type; where the call is awaited and that is the value inside the task, the task is made here.
/// </remarks>
internal sealed class MockHandle
{
    private static readonly ProxyGenerator _generator = new();
    private static readonly ProxyGenerationOptions _options = new(new MockHook());
    private static readonly ConditionalWeakTable<object, MockHandle> _handles = [];

    private readonly FluentDefaultProvider _defaults;
    private readonly MockRegistry _mocks;
    private readonly MockHandle? _shared;
    private readonly List<MockInvocation> _invocations = [];
    private readonly List<CallSetup> _setups = [];
    private readonly MockChildren _children;
    private readonly object? _instance;

    internal MockHandle(Type mockedType, FluentDefaultProvider defaults, MockRegistry mocks, MockHandle? shared = null)
    {
        MockedType = mockedType;
        _defaults = defaults;
        _mocks = mocks;
        _shared = shared;
        _children = new(defaults, mocks, shared?._children);
        _instance = Create(mockedType);
        _handles.Add(_instance, this);
    }

    internal Type MockedType { get; }

    internal object Instance => _instance!;

    /// Calls made while arranging are logged, but not counted.
    internal IReadOnlyList<MockInvocation> ActInvocations => [.. Invocations.Where(call => call.InAct)];

    private IReadOnlyList<MockInvocation> Invocations
    {
        get
        {
            lock (_invocations)
                return [.. _invocations];
        }
    }

    internal void Answer<TService>(Expression<Action<TService>> call, Func<IReadOnlyList<object>, object?> answer)
        where TService : class
        => Prepare(call, typeof(void), answer)(this);

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
        => SetUp(CallMatcher.For(member), answerType, answer);

    internal int CountCalls(LambdaExpression call) => Counting(call)(Invocations);

    private IReadOnlyList<MockInvocation> OwnInvocations => [.. Invocations.Where(call => call.Receiver == this)];

    /// <summary>
    /// A count made ready to take over a mock's calls; every step of a chain is read now. The rest of a
    /// chain is counted on each mock its first step answered with, once, among the calls that mock
    /// received itself. A step taken while arranging still leads on; only the act's calls are counted.
    /// </summary>
    private static Func<IEnumerable<MockInvocation>, int> Counting(LambdaExpression call)
    {
        if (!CallChain.TrySplit(call, out var firstStep, out var rest))
        {
            var matcher = CallMatcher.For(call);
            return calls => calls.Count(called => called.InAct && matcher.Matches(called));
        }

        var step = CallMatcher.For(firstStep);
        var countRest = Counting(rest);
        return calls => calls
            .Where(step.Matches)
            .Select(reached => HandleOf(reached.Answer))
            .OfType<MockHandle>()
            .Distinct()
            .Sum(mock => countRest(mock.OwnInvocations));
    }

    private static MockHandle? HandleOf(object? answer)
        => AsyncAnswer.ValueOf(answer) is { } value && _handles.TryGetValue(value, out var mock) ? mock : null;

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
            return mock => mock.SetUp(matcher, answerType, answer);
        }

        var step = CallMatcher.For(firstStep);
        var childType = firstStep.Body.Type;
        var setUpChild = Prepare(rest, answerType, answer);
        return mock => mock.SetUpChain(step, childType, setUpChild);
    }

    private void SetUpChain(CallMatcher firstStep, Type childType, Action<MockHandle> setUpChild)
    {
        _children.Add(firstStep, setUpChild);
        SetUp(firstStep, childType, arguments => _children.At(firstStep.Method, childType, arguments).Instance);
    }

    private void SetUp(CallMatcher matcher, Type answerType, Func<IReadOnlyList<object>, object?> answer)
    {
        lock (_setups)
            _setups.Add(new(
                matcher, arguments => AsyncAnswer.Respond(matcher.ReturnType, answerType, () => answer(arguments!))));
    }

    /// A call is logged before it is answered, so whatever answers it may read the log; what it answered
    /// with is logged after.
    private object? Receive(MethodInfo method, object?[] arguments)
    {
        var invocation = new MockInvocation(method, [.. arguments], this, _mocks.ActHasBegun);
        Log(invocation);
        invocation.Answer = Respond(method, arguments);
        return invocation.Answer;
    }

    /// <summary>
    /// The latest setup matching a call answers it; a call no setup matches is answered by TSpec's
    /// defaults, except one made while the instance is still being constructed, which has no mock to be
    /// answered for yet and gets its type's default.
    /// </summary>
    private object? Respond(MethodInfo method, object?[] arguments)
    {
        var returnType = method.ReturnType;
        if (LatestMatching(method, arguments) is { } setup)
        {
            setup.Matcher.WriteOutArguments(arguments);
            return setup.Respond(arguments) ?? DefaultOf(returnType);
        }
        if (returnType == typeof(void))
            return null;
        if (_instance is null)
            return DefaultOf(returnType);
        return _defaults.GetDefaultValue(returnType, this) ?? DefaultOf(returnType);
    }

    /// A child's call is logged on the shared mock of its type too, which so counts every call to the type.
    private void Log(MockInvocation invocation)
    {
        lock (_invocations)
            _invocations.Add(invocation);
        _shared?.Log(invocation);
    }

    /// A child's own setups come first; a call they leave unanswered goes to the shared mock's setups.
    private CallSetup? LatestMatching(MethodInfo method, object?[] arguments)
        => OwnLatestMatching(method, arguments) ?? _shared?.LatestMatching(method, arguments);

    private CallSetup? OwnLatestMatching(MethodInfo method, object?[] arguments)
    {
        lock (_setups)
            return _setups.LastOrDefault(setup => setup.Matcher.Matches(method, arguments));
    }

    private static object? DefaultOf(Type type)
        => type.IsValueType && type != typeof(void) ? Activator.CreateInstance(type) : null;

    private object Create(Type type)
    {
        if (typeof(Delegate).IsAssignableFrom(type))
            return DelegateForwarder.Create(type, Receive);

        if (type.IsInterface)
            return _generator.CreateClassProxy(typeof(object), [type], _options, new Interceptor(this));

        if (type.IsSealed)
            throw new SetupFailed($"{type.Alias()} is sealed, so it cannot be mocked. Provide one with Using instead");

        return _generator.CreateClassProxy(type, _options, new Interceptor(this));
    }

    private sealed record CallSetup(CallMatcher Matcher, Func<object?[], object?> Respond);

    private sealed class Interceptor(MockHandle mock) : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.DeclaringType == typeof(object))
            {
                invocation.ReturnValue = mock.MockedType.Alias();
                return;
            }
            invocation.ReturnValue = mock.Receive(invocation.GetConcreteMethod(), invocation.Arguments);
        }
    }

    /// <summary>
    /// Every member the mocked type lets a proxy override is intercepted. Of what every object has,
    /// only ToString is, so a mock names itself as the specification names it; equality stays the
    /// instance's own.
    /// </summary>
    private sealed class MockHook : IProxyGenerationHook
    {
        public void MethodsInspected() { }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo) { }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
            => methodInfo.DeclaringType != typeof(object) || methodInfo.Name == nameof(ToString);

        public override bool Equals(object? obj) => obj is MockHook;

        public override int GetHashCode() => typeof(MockHook).GetHashCode();
    }
}

internal sealed record MockInvocation(MethodInfo Method, IReadOnlyList<object?> Arguments, MockHandle Receiver, bool InAct)
{
    internal object? Answer { get; set; }
}
