namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class MockingStrategy(FluentDefaultProvider fluentDefaultProvider) : IGenerationStrategy
{
    private readonly FluentDefaultProvider _defaults = fluentDefaultProvider;
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
        if (!IsMockable(request) || !IsArranged(request.Type))
            return false;

        result = _registry.GetMock(request.Type).Instance;
        return true;
    }

    private bool IsArranged(Type type) => _registry.HasMock(type) || _defaults.IsSetUp(type);

    internal static bool IsMockable(Type type)
        => type.IsInterface
        || type.IsAbstract
        || typeof(Delegate).IsAssignableFrom(type);

    private static bool IsMockable(GenerationRequest request) => IsMockable(request.Type);
}
