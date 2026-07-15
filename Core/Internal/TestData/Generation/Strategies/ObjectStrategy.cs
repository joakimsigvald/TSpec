using System.Collections.Concurrent;
using TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

namespace TSpec.Internal.TestData.Generation.Strategies;

internal class ObjectStrategy : IGenerationStrategy
{
    private static readonly ConcurrentDictionary<Type, object?> _defaultCache = [];
    private static readonly Func<Type, object?> _defaultFactory =
        t => t.IsValueType ? Activator.CreateInstance(t) : null;

    public bool TryGenerate(GenerationRequest request, ref object? result)
    {
        var type = request.Type;
        var stack = request.Stack;
        result = InstantiateWithConstructor()
            ?? InstantiateWithConversionOperator()
            ?? (type.IsValueType ? Activator.CreateInstance(request.Type) : null);
        if (result != null)
            PopulatePublicProperties(result);

        return result is not null;

        object? InstantiateWithConstructor()
        {
            var compiled = ConstructorCompiler.Get(type);
            if (compiled.Instantiate is null)
                return null;

            var args = new object?[compiled.ParameterTypes.Length];
            for (int i = 0; i < compiled.ParameterTypes.Length; i++)
                args[i] = request.Next.Create(compiled.ParameterTypes[i]);
            try
            {
                return compiled.Instantiate(args!);
            }
            catch (Exception ex)
            {
                try
                {
                    return Activator.CreateInstance(type)!;
                }
                catch (Exception)
                {
                    throw new SetupFailed($"Failed to create value for type {type.Name}", ex);
                }
            }
        }

        object? InstantiateWithConversionOperator()
        {
            var compiled = ConversionOperatorCompiler.Get(type);
            if (compiled.Instantiate is null || compiled.ParameterType is null)
                return null;

            var paramValue = request.Next.Create(compiled.ParameterType);
            return paramValue is not null
                ? compiled.Instantiate(paramValue)
                : null;
        }

        void PopulatePublicProperties(object instance)
        {
            var accessors = PropertyCompiler.GetAccessors(type);
            foreach (var accessor in accessors)
            {
                var currentValue = accessor.Get(instance);
                var emptyValue = _defaultCache.GetOrAdd(accessor.PropertyType, _defaultFactory);
                if (Equals(emptyValue, currentValue))
                {
                    var newValue = request.Next.Create(accessor.PropertyType);
                    accessor.Set(instance, newValue);
                }
            }
        }
    }
}