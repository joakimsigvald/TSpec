using System.Linq.Expressions;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Mocking;

/// <summary>
/// What every mock of a type shares: the setups made on the type, and the calls all of them received.
/// It makes the mocks, and is what <c>Given&lt;T&gt;()</c> and <c>Then&lt;T&gt;()</c> reach.
/// </summary>
internal sealed class MockFamily : IMocked
{
    private readonly MockRegistry _mocks;

    internal MockFamily(Type type, MockRegistry mocks)
    {
        if (!MockableTypes.IsMockable(type))
            throw new SetupFailed($"{type.Alias()} is sealed, so it cannot be mocked. Provide one with Using instead");

        MockedType = type;
        _mocks = mocks;
    }

    internal Type MockedType { get; }

    internal CallLog Log { get; } = new(null);

    public string Name => MockedType.Alias();

    public CallSetups Setups { get; } = new(null);

    public IReadOnlyList<MockInvocation> CountedInvocations => Log.Counted;

    /// A mock of the type, which reads as the type until a mention names it.
    internal MockHandle NewMock() => new(this, _mocks, MockedType.Alias(), MockedType.Alias());

    /// A mock a call answered with, named by that call.
    internal MockHandle NewChild(string madeBy) => new(this, _mocks, $"{MockedType.Alias()} from {madeBy}", madeBy);

    public int CountCalls(LambdaExpression call, string callExpr) => Log.Count(call, callExpr);
}
