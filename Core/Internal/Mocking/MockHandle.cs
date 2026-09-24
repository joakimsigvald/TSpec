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
    /// The latest setup matching a call answers it. Failing one, what was kept at the call's address —
    /// a property's last set — answers it before a setup by name does, since a name is a default; a
    /// call nothing answers is answered once per address and answers the same from then on. A call
    /// made while the instance is still being constructed has no mock to be answered for yet, so
    /// nothing is kept for it and it gets its type's default.
    /// </summary>
    private object? RespondToCall(MethodInfo method, object?[] arguments)
    {
        var returnType = method.ReturnType;
        if (Setups.TryAnswer(this, method, arguments, out var answer))
            return answer ?? DefaultOf(returnType);

        var address = CallAddress.Of(method, arguments);
        if (TryReadKept(returnType, address, out var kept))
            return kept;

        if (Setups.TryAnswerByName(this, method, arguments, out answer))
            return answer ?? DefaultOf(returnType);

        return Unanswered(returnType, address);
    }

    private bool TryReadKept(Type returnType, CallAddress address, out object? kept)
    {
        kept = null;
        return returnType != typeof(void) && _instance is not null && _answers.TryRead(address, out kept);
    }

    private object? Unanswered(Type returnType, CallAddress address)
    {
        if (returnType == typeof(void))
            return null;

        if (_instance is null)
            return DefaultOf(returnType);

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
