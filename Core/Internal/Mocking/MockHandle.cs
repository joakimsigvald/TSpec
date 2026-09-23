using System.Linq.Expressions;
using System.Reflection;

namespace TSpec.Internal.Mocking;

/// <summary>
/// TSpec's hold on one mock: the instance handed out, and the calls that instance received. Castle
/// makes the instance; every call it receives is logged, then answered by the latest setup that
/// matches it, or else by TSpec's defaults. What its type was set up with applies to it too.
/// </summary>
internal sealed class MockHandle : IMocked
{
    private readonly MockFamily _family;
    private readonly MockRegistry _mocks;
    private readonly CallLog _log;
    private readonly MockChildren _children = new();
    private readonly KeptAnswers _answers = new();
    private readonly object? _instance;

    internal MockHandle(MockFamily family, MockRegistry mocks, string name, string path)
    {
        _family = family;
        _mocks = mocks;
        Name = name;
        Path = path;
        _log = new(family.Log);
        Setups = new(family.Setups);
        _instance = MockInstance.Create(
            MockedType, () => Name, Receive, () => mocks.Defaults.GetConstructorArguments(MockedType));
        _log.RecordsCallsOf(_instance);
        mocks.Add(this);
    }

    internal Type MockedType => _family.MockedType;

    /// How the mock names itself: by its type, by the mention that took it, or by the call that made it.
    public string Name { get; private set; }

    /// How a call on the mock is written, which the names of its children compose on.
    private string Path { get; set; }

    internal object Instance => _instance!;

    /// What this mock alone was set up with, before what its family was.
    public CallSetups Setups { get; }

    public IReadOnlyList<MockInvocation> CountedInvocations => _log.Counted;

    public int CountCalls(LambdaExpression call, string callExpr) => _log.Count(call, callExpr);

    internal void Mentioned(string name) => Name = Path = name;

    /// The mock a call reached at an address, made the first time with every chain whose first step matches it.
    internal MockHandle ChildAt(MethodInfo method, Type childType, IReadOnlyList<object?> arguments)
        => _children.At(method, arguments, () => NewChild(method, childType, arguments));

    private MockHandle NewChild(MethodInfo method, Type childType, IReadOnlyList<object?> arguments)
    {
        var child = _mocks.GetMockFamily(childType).NewChild(ReceivedCalls.Describe(Path, method, arguments));
        foreach (var setUp in Setups.ChainsFor(method, arguments))
            setUp(child.Setups);
        return child;
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
        if (Setups.TryAnswer(this, method, arguments, out var answer))
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

    /// A call answered with a new mock is answered with one mock per address instead, as a chained
    /// setup is, so what is reached through other arguments is counted apart.
    internal object? PerAddress(Type type, object? value, CallAddress address)
        => _mocks.IsUnnamed(value)
            ? ChildAt(address.Member, type, address.Arguments).Instance
            : value;

    private static object? DefaultOf(Type type)
        => type.IsValueType && type != typeof(void) ? Activator.CreateInstance(type) : null;
}
