namespace TSpec.Internal.TestData.Generation.Strategies;

internal class EnumStrategy : IGenerationStrategy
{
    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        if (!request.Type.IsEnum)
            return false;

        var values = Enum.GetValues(request.Type);
        result = values.Length > 0 ? values.GetValue(0)! : Activator.CreateInstance(request.Type)!;
        return true;
    }
}