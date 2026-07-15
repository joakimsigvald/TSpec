using System.Reflection;
using TSpec.Semantic;

namespace TSpec.Internal.TestData.Generation.Strategies;

internal class SemanticTypeStrategy(Counter counter) : IGenerationStrategy
{
    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        var type = request.Type;
        if (!HasSemanticInterface(type))
            return false;

        var generateMethod = type.GetMethod("GenerateValue", BindingFlags.Public | BindingFlags.Static);
        var primitiveVal = generateMethod!.Invoke(null, [counter.Next]);
        result = Activator.CreateInstance(type, primitiveVal);
        return true;
    }

    private static bool HasSemanticInterface(Type type)
        => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISemantic<>));
}