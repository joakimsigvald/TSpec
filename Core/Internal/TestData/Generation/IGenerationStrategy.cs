namespace TSpec.Internal.TestData.Generation;

internal interface IGenerationStrategy
{
    bool TryGenerate(GenerationRequest request, ref object? result);
}