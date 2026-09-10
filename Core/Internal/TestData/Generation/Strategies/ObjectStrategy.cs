using System.Collections.Concurrent;
using TSpec.Internal.Specification;
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
        List<string>? honouredDefaults = null;
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

            var args = new object?[compiled.Parameters.Length];
            for (int i = 0; i < compiled.Parameters.Length; i++)
                args[i] = Fill(compiled.Parameters[i]);
            try
            {
                return compiled.Instantiate(args!);
            }
            catch (Exception ex)
            {
                honouredDefaults = null;
                object instance;
                try
                {
                    instance = Activator.CreateInstance(type)!;
                }
                catch (Exception)
                {
                    throw new SetupFailed(
                        $"Failed to create value for type {type.Name}. Arrange it with Using<{type.Name}>(...) or Given().A<{type.Name}>(...)",
                        ex);
                }
                SpecificationContext.Current.AddSetupWarning(
                    $"{type.Name}: the constructor rejected the generated arguments ({ex.GetType().Name}), "
                    + $"so the parameterless constructor was used instead. "
                    + $"Arrange it with Using<{type.Name}>(...) or Given().A<{type.Name}>(...) if that is not what you want.");
                return instance;
            }
        }

        // A parameter with a default value says what the class runs without, so building the
        // subject fills it with what the test arranged and lets the default stand for the rest.
        object? Fill(CompiledParameter parameter)
        {
            if (!parameter.HasDefault || !request.Scope.HasFlag(For.Subject))
                return request.Next.Create(parameter.Type);
            if (request.Next.TryCreateFromSetup(parameter.Type, out var arranged))
                return arranged;

            (honouredDefaults ??= []).Add(parameter.Name);
            return parameter.Default;
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
                // A property fed by a default the constructor was allowed to keep already carries
                // a value: false, 0 and null look like empty slots but are what the class chose.
                if (CarriesAHonouredDefault(accessor.Name))
                    continue;

                var currentValue = accessor.Get(instance);
                var emptyValue = _defaultCache.GetOrAdd(accessor.PropertyType, _defaultFactory);
                if (Equals(emptyValue, currentValue))
                {
                    var newValue = request.Next.Create(accessor.PropertyType);
                    accessor.Set(instance, newValue);
                }
            }
        }

        bool CarriesAHonouredDefault(string propertyName)
            => honouredDefaults?.Any(parameterName => string.Equals(parameterName, propertyName, StringComparison.OrdinalIgnoreCase)) ?? false;
    }
}