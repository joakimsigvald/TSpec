using Moq;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class MockingStrategy(FluentDefaultProvider fluentDefaultProvider) : IGenerationStrategy
{
    private readonly MockRegistry _registry = new(fluentDefaultProvider);

    internal Mock GetMock(Type type) => _registry.GetMock(type);
    internal Mock<TObject> GetMock<TObject>() where TObject : class => _registry.GetMock<TObject>();

    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        if (request.WithDefaultFallback && IsMockingResponsibility(request))
        {
            result = _registry.GetMock(request.Type).Object;
            return true;
        }
        return false;
    }

    private static bool IsMockingResponsibility(GenerationRequest request)
        => request.Type.IsInterface
        || request.Type.IsAbstract
        || typeof(Delegate).IsAssignableFrom(request.Type);
}