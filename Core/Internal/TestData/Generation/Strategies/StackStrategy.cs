namespace TSpec.Internal.TestData.Generation.Strategies;

internal class StackStrategy : IGenerationStrategy
{
    public bool TryGenerate(GenerationRequest request, ref object? result)
        => request.Stack.Contains(request.Type);
}