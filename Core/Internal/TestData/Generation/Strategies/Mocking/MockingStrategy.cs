using TSpec.Internal.Pipelines;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class MockingStrategy(
    FluentDefaultProvider fluentDefaultProvider, IPipelinePhase phase, SetupLambda setupLambda) : IGenerationStrategy
{
    private readonly FluentDefaultProvider _defaults = fluentDefaultProvider;
    private readonly MockRegistry _registry = new(fluentDefaultProvider, phase, setupLambda);

    internal MockHandle GetMock(Type type) => _registry.GetMock(type);

    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        if (!ShouldMock(request))
            return false;

        result = _registry.GetMock(request.Type).Instance;
        return true;
    }

    private bool ShouldMock(GenerationRequest request)
        => request.WithDefaultFallback && ShouldMock(request.Type);

    internal bool TryUseArrangedMock(GenerationRequest request, ref object? result)
    {
        if (!IsArranged(request.Type))
            return false;

        result = _registry.GetMock(request.Type).Instance;
        return true;
    }

    private bool ShouldMock(Type type) => IsMockedByDefault(type) || IsArranged(type);

    private bool IsArranged(Type type) => _registry.HasMock(type) || _defaults.IsSetUp(type);

    internal static bool IsMockedByDefault(Type type)
        => type.IsInterface
        || type.IsAbstract
        || typeof(Delegate).IsAssignableFrom(type);
}
