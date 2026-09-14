namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class MockingStrategy(FluentDefaultProvider fluentDefaultProvider) : IGenerationStrategy
{
    private readonly MockRegistry _registry = new(fluentDefaultProvider);

    internal MockHandle GetMock(Type type) => _registry.GetMock(type);

    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        if (request.WithDefaultFallback && IsMockable(request))
        {
            result = _registry.GetMock(request.Type).Instance;
            return true;
        }
        return false;
    }

    internal bool TryUseArrangedMock(GenerationRequest request, ref object? result)
    {
        if (!IsMockable(request) || !_registry.HasMock(request.Type))
            return false;

        result = _registry.GetMock(request.Type).Instance;
        return true;
    }

    internal static bool IsMockable(Type type)
        => type.IsInterface
        || type.IsAbstract
        || typeof(Delegate).IsAssignableFrom(type);

    private static bool IsMockable(GenerationRequest request) => IsMockable(request.Type);
}
