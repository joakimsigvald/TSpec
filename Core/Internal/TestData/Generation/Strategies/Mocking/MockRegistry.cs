using System.Collections.Concurrent;
namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class MockRegistry(FluentDefaultProvider defaultProvider)
{
    private readonly ConcurrentDictionary<Type, MockHandle> _mocks = [];

    internal MockHandle GetMock(Type type) => _mocks.GetOrAdd(type, CreateMock);

    internal bool HasMock(Type type) => _mocks.ContainsKey(type);

    internal bool ActHasBegun { get; private set; }

    internal void BeginAct() => ActHasBegun = true;

    private MockHandle CreateMock(Type type) => new(type, defaultProvider, this);
}
