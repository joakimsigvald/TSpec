using TSpec.Internal.Mocking;
using TSpec.Internal.Pipelines;

namespace TSpec.Internal.TestData.Generation.Strategies;

internal class MockingStrategy(
    FluentDefaultProvider fluentDefaultProvider, IPipelinePhase phase, SetupLambda setupLambda) : IGenerationStrategy
{
    private readonly FluentDefaultProvider _defaults = fluentDefaultProvider;
    private readonly MockRegistry _registry = new(fluentDefaultProvider, phase, setupLambda);

    internal MockFamily GetMockFamily(Type type) => _registry.GetMockFamily(type);

    internal MockHandle MockOf(object? value, string mentionName) => _registry.MockOf(value, mentionName);

    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        if (!ShouldMock(request.Type))
            return false;

        result = _registry.NewMock(request.Type).Instance;
        return true;
    }

    internal bool TryUseArrangedMock(GenerationRequest request, ref object? result)
    {
        if (!IsArranged(request.Type))
            return false;

        result = _registry.NewMock(request.Type).Instance;
        return true;
    }

    internal void Name(object? value, Func<string> name) => _registry.Name(value, name);

    private bool ShouldMock(Type type) => MockableTypes.IsMockedByDefault(type) || IsArranged(type);

    private bool IsArranged(Type type) => _registry.HasMockFamily(type) || _defaults.IsSetUp(type);
}
