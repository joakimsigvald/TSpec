using System.Collections.Concurrent;
using TSpec.Internal.Pipelines;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class MockRegistry(FluentDefaultProvider defaultProvider, IPipelinePhase phase, SetupLambda setupLambda)
{
    private readonly ConcurrentDictionary<Type, MockHandle> _mocks = [];

    internal MockHandle GetMock(Type type) => _mocks.GetOrAdd(type, CreateMock);

    internal bool HasMock(Type type) => _mocks.ContainsKey(type);

    internal Phase Phase => phase.Current;

    internal FluentDefaultProvider Defaults => defaultProvider;

    internal SetupGuard Guard { get; } = new(phase, setupLambda);

    private MockHandle CreateMock(Type type) => new(type, this);
}
