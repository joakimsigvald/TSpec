namespace TSpec.Internal.TestData.Generation.Strategies;

internal class EnumStrategy(Counter counter) : IGenerationStrategy
{
    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        if (!request.Type.IsEnum)
            return false;

        var values = Enum.GetValues(request.Type);
        result = values.Length > 0
            ? values.GetValue(counter.Next % values.Length)!
            : Activator.CreateInstance(request.Type)!;
        return true;
    }
}