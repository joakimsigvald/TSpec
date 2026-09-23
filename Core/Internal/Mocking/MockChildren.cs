using System.Reflection;

namespace TSpec.Internal.Mocking;

/// <summary>
/// The children a mock answers chained calls with. A child is reached at an address — the member and
/// the arguments it was called with — and is made the first time its address is reached.
/// </summary>
internal sealed class MockChildren
{
    private readonly List<Child> _children = [];

    internal MockHandle At(MethodInfo method, IReadOnlyList<object?> arguments, Func<MockHandle> newChild)
    {
        lock (_children)
        {
            if (_children.FirstOrDefault(child => child.Address.Matches(method, arguments)) is { } reached)
                return reached.Mock;

            var mock = newChild();
            _children.Add(new(CallMatcher.Exactly(method, arguments), mock));
            return mock;
        }
    }

    private sealed record Child(CallMatcher Address, MockHandle Mock);
}
