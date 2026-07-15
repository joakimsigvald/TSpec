namespace TSpec.Internal.TestData.Generation.Strategies;

internal class NullableStrategy : IGenerationStrategy
{
    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        var underlyingType = Nullable.GetUnderlyingType(request.Type);
        if (underlyingType is null)
            return false;

        result = request.Create(underlyingType);
        return true;
    }
}