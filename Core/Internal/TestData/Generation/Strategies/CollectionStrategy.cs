namespace TSpec.Internal.TestData.Generation.Strategies;

internal class CollectionStrategy : IGenerationStrategy
{
    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        var type = request.Type;
        if (type.IsArray)
            result = Array.CreateInstance(type.GetElementType()!, 0);
        else if (IsGenericEnumerable(type))
            result = Array.CreateInstance(type.GetGenericArguments()[0], 0);
        else return false;
        return true;
    }

    private static bool IsGenericEnumerable(Type type)
    {
        if (!type.IsGenericType) return false;
        var genericTypeDef = type.GetGenericTypeDefinition();
        return genericTypeDef == typeof(IEnumerable<>) ||
               genericTypeDef == typeof(IList<>) ||
               genericTypeDef == typeof(ICollection<>) ||
               genericTypeDef == typeof(IReadOnlyCollection<>) ||
               genericTypeDef == typeof(IReadOnlyList<>);
    }
}