using System.Collections.Concurrent;
using TSpec.Internal.Pipelines;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class MockRegistry(FluentDefaultProvider defaultProvider, IPipelinePhase phase, SetupLambda setupLambda)
{
    private readonly ConcurrentDictionary<Type, MockHandle> _mocks = [];

    internal MockHandle GetMock(Type type) => _mocks.GetOrAdd(type, CreateMock);

    internal bool HasMock(Type type) => _mocks.ContainsKey(type);

    /// Whether a value is the one mock of its type, which a call answers with one of its own children instead.
    internal bool IsTypeMock(Type type, object? value)
        => value is not null && HasMock(type) && ReferenceEquals(GetMock(type).Instance, value);

    internal Phase Phase => phase.Current;

    internal FluentDefaultProvider Defaults => defaultProvider;

    internal SetupGuard Guard { get; } = new(phase, setupLambda);

    private MockHandle CreateMock(Type type) => new(type, this);
}
