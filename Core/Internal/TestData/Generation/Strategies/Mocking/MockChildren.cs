using System.Reflection;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// The children a mock answers chained calls with. A child is reached at an address — the member and
/// the arguments it was called with — and is made the first time its address is reached, with every
/// chained setup whose first step matches it.
/// </summary>
internal sealed class MockChildren(MockRegistry mocks, MockChildren? sharedChildren)
{
    private readonly List<ChainedSetup> _chains = [];
    private readonly List<Child> _children = [];

    internal void Add(CallMatcher firstStep, Action<MockHandle> setUpChild)
    {
        lock (_chains)
            _chains.Add(new(firstStep, setUpChild));
    }

    internal MockHandle At(MethodInfo method, Type childType, IReadOnlyList<object> arguments)
    {
        lock (_children)
        {
            if (_children.FirstOrDefault(child => child.Address.Matches(method, arguments)) is { } reached)
                return reached.Mock;

            var mock = new MockHandle(childType, mocks, mocks.GetMock(childType));
            _children.Add(new(CallMatcher.Exactly(method, arguments), mock));
            foreach (var chain in ChainsFor(method, arguments))
                chain.SetUpChild(mock);
            return mock;
        }
    }

    /// The chained setups made on the type come first, so those made on this mock win.
    private IEnumerable<ChainedSetup> ChainsFor(MethodInfo method, IReadOnlyList<object> arguments)
        => (sharedChildren?.ChainsMatching(method, arguments) ?? []).Concat(ChainsMatching(method, arguments));

    private ChainedSetup[] ChainsMatching(MethodInfo method, IReadOnlyList<object> arguments)
    {
        lock (_chains)
            return [.. _chains.Where(chain => chain.FirstStep.Matches(method, arguments))];
    }

    private sealed record ChainedSetup(CallMatcher FirstStep, Action<MockHandle> SetUpChild);

    private sealed record Child(CallMatcher Address, MockHandle Mock);
}
