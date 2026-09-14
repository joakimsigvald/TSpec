namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class MockingStrategy(FluentDefaultProvider fluentDefaultProvider) : IGenerationStrategy
{
    private readonly MockRegistry _registry = new(fluentDefaultProvider);

    internal MockHandle GetMock(Type type) => _registry.GetMock(type);

    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        if (request.WithDefaultFallback && IsMockingResponsibility(request))
        {
            result = _registry.GetMock(request.Type).Instance;
            return true;
        }
        return false;
    }

    internal bool TryUseArrangedMock(GenerationRequest request, ref object? result)
    {
        if (!IsMockingResponsibility(request) || !_registry.HasMock(request.Type))
            return false;

        result = _registry.GetMock(request.Type).Instance;
        return true;
    }

    internal static bool IsMocked(Type type)
        => type.IsInterface
        || type.IsAbstract
        || typeof(Delegate).IsAssignableFrom(type);

    private static bool IsMockingResponsibility(GenerationRequest request) => IsMocked(request.Type);
}
