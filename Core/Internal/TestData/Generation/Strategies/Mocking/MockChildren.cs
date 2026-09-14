using System.Reflection;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// The children a mock answers chained calls with. A child is reached at an address — the member and
/// the arguments it was called with — and is made the first time its address is reached, with every
/// chained setup whose first step matches it.
/// </summary>
internal sealed class MockChildren(FluentDefaultProvider defaults, MockRegistry mocks)
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

            var mock = new MockHandle(childType, defaults, mocks, mocks.GetMock(childType));
            _children.Add(new(CallMatcher.Exactly(method, arguments), mock));
            foreach (var chain in ChainsMatching(method, arguments))
                chain.SetUpChild(mock);
            return mock;
        }
    }

    private ChainedSetup[] ChainsMatching(MethodInfo method, IReadOnlyList<object> arguments)
    {
        lock (_chains)
            return [.. _chains.Where(chain => chain.FirstStep.Matches(method, arguments))];
    }

    private sealed record ChainedSetup(CallMatcher FirstStep, Action<MockHandle> SetUpChild);

    private sealed record Child(CallMatcher Address, MockHandle Mock);
}
