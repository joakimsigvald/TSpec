namespace TSpec.Internal.TestData.Generation.Strategies;

internal class DefaultStrategy(IRepository repository) : IGenerationStrategy
{
    public bool TryGenerate(GenerationRequest request, ref object? result)
        => request.WithDefaultFallback && repository.TryGetDefault(request.Type, request.Scope, out result);
}