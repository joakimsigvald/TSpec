using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using TSpec.Internal.Pipelines;
using TSpec.Internal.Specification;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class MockRegistry(FluentDefaultProvider defaultProvider, IPipelinePhase phase, SetupLambda setupLambda)
{
    private readonly ConcurrentDictionary<Type, MockFamily> _families = [];
    private readonly ConditionalWeakTable<object, MockHandle> _mocks = [];
    private readonly ConditionalWeakTable<object, MockHandle> _unnamed = [];

    internal MockFamily GetMockFamily(Type type) => _families.GetOrAdd(type, CreateFamily);

    internal bool HasMockFamily(Type type) => _families.ContainsKey(type);

    internal void Add(MockHandle mock) => _mocks.Add(mock.Instance, mock);

    /// The mock a mention holds, which only a mock TSpec made can be.
    internal MockHandle MockOf(object? value, string mentionName)
        => value is not null && _mocks.TryGetValue(value, out var mock)
            ? mock
            : throw new SetupFailed(
                $"{mentionName.Capitalize()} is {Described(value)}, which TSpec does not mock. Set it up where it was made");

    private static string Described(object? value) => value is null ? "null" : $"a real {value.GetType().Alias()}";

    internal MockHandle NewMock(Type type)
    {
        var mock = GetMockFamily(type).NewMock();
        _unnamed.Add(mock.Instance, mock);
        return mock;
    }

    /// Whether a value is a mock no mention has taken, which a call answers with one of its own children instead.
    internal bool IsUnnamed(object? value) => value is not null && _unnamed.TryGetValue(value, out _);

    /// A mock is named by the first mention that takes it, and keeps that name.
    internal void Name(object? value, Func<string> name)
    {
        if (value is null || !_unnamed.TryGetValue(value, out var mock))
            return;

        _unnamed.Remove(value);
        mock.Mentioned(name());
    }

    internal Phase Phase => phase.Current;

    internal FluentDefaultProvider Defaults => defaultProvider;

    internal SetupGuard Guard { get; } = new(phase, setupLambda);

    private MockFamily CreateFamily(Type type) => new(type, this);
}
