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
        if (!ShouldMock(request.Type))
            return false;

        result = _registry.GetMock(request.Type).Instance;
        return true;
    }

    internal bool TryUseArrangedMock(GenerationRequest request, ref object? result)
    {
        if (!IsArranged(request.Type))
            return false;

        result = _registry.GetMock(request.Type).Instance;
        return true;
    }

    private bool ShouldMock(Type type) => IsMockedByDefault(type) || IsArranged(type);

    private bool IsArranged(Type type) => _registry.HasMock(type) || _defaults.IsSetUp(type);

    /// A class the test sets up is mocked as well, so every type but a sealed class or a value can be.
    internal static bool IsMockable(Type type)
        => IsMockedByDefault(type) || type is { IsClass: true, IsSealed: false };

    internal static bool IsMockedByDefault(Type type)
        => type.IsInterface
        || type.IsAbstract
        || typeof(Delegate).IsAssignableFrom(type);
    }
