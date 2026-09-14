using Castle.DynamicProxy;
using System.Linq.Expressions;
using System.Reflection;
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

    private readonly FluentDefaultProvider _defaults;
    private readonly MockRegistry _mocks;
    private readonly List<MockInvocation> _invocations = [];
    private readonly List<CallSetup> _setups = [];
    private readonly object? _instance;

    internal MockHandle(Type mockedType, FluentDefaultProvider defaults, MockRegistry mocks)
    {
        MockedType = mockedType;
        _defaults = defaults;
        _mocks = mocks;
        _instance = Create(mockedType);
    }

    internal Type MockedType { get; }

    internal object Instance => _instance!;

    internal IReadOnlyList<MockInvocation> Invocations
    {
        get
        {
            lock (_invocations)
                return [.. _invocations];
        }
    }

    internal void Answer<TService>(Expression<Action<TService>> call, Func<IReadOnlyList<object>, object?> answer)
        where TService : class
        => SetUp(call, typeof(void), answer);

    internal void Answer<TService, TResult>(
        Expression<Func<TService, TResult>> call, Type answerType, Func<IReadOnlyList<object>, object?> answer)
        where TService : class
        => SetUp(call, answerType, answer);

    /// <summary>
    /// A member no expression can name — a protected method or property. A name states no
    /// arguments, so every parameter takes whatever it is passed.
    /// </summary>
    internal void Answer<TService>(MemberInfo member, Type answerType, Func<IReadOnlyList<object>, object?> answer)
        where TService : class
        => SetUp(CallMatcher.For(member), answerType, answer);

    internal int CountCalls(LambdaExpression call)
    {
        if (CallChain.TrySplit(call, out _, out var last))
            return MockOf(last).CountCalls(last);

        var matcher = CallMatcher.For(call);
        return Invocations.Count(matcher.Matches);
    }

    private void SetUp(LambdaExpression call, Type answerType, Func<IReadOnlyList<object>, object?> answer)
    {
        if (CallChain.TrySplit(call, out var link, out var last))
            SetUpChain(link, last, answerType, answer);
        else
            SetUp(CallMatcher.For(call), answerType, answer);
    }

    private void SetUpChain(
        LambdaExpression link, LambdaExpression last, Type answerType, Func<IReadOnlyList<object>, object?> answer)
    {
        var child = MockOf(last);
        SetUp(link, child.MockedType, _ => child.Instance);
        child.SetUp(last, answerType, answer);
    }

    private MockHandle MockOf(LambdaExpression call) => _mocks.GetMock(call.Parameters[0].Type);

    private void SetUp(CallMatcher matcher, Type answerType, Func<IReadOnlyList<object>, object?> answer)
    {
        lock (_setups)
            _setups.Add(new(
                matcher, arguments => AsyncAnswer.Respond(matcher.ReturnType, answerType, () => answer(arguments!))));
    }

    /// <summary>
    /// A call is logged before it is answered, so whatever answers it may read the log. The latest
    /// setup matching it answers; a call no setup matches is answered by TSpec's defaults, except
    /// one made while the instance is still being constructed, which has no mock to be answered for
    /// yet and gets its type's default.
    /// </summary>
    private object? Receive(MethodInfo method, object?[] arguments)
    {
        lock (_invocations)
            _invocations.Add(new MockInvocation(method, [.. arguments]));
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

    private CallSetup? LatestMatching(MethodInfo method, object?[] arguments)
    {
        lock (_setups)
            return _setups.LastOrDefault(setup => setup.Matcher.Matches(method, arguments));
    }

    private static object? DefaultOf(Type type)
        => type.IsValueType && type != typeof(void) ? Activator.CreateInstance(type) : null;

    private object Create(Type type)
        => typeof(Delegate).IsAssignableFrom(type) ? DelegateForwarder.Create(type, Receive)
        : type.IsInterface ? _generator.CreateClassProxy(typeof(object), [type], _options, new Interceptor(this))
        : _generator.CreateClassProxy(type, _options, new Interceptor(this));

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

internal sealed record MockInvocation(MethodInfo Method, IReadOnlyList<object?> Arguments);
