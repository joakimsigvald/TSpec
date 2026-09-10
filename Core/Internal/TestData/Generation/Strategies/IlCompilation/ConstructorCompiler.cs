using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

internal static class ConstructorCompiler
{
    private static readonly ConcurrentDictionary<Type, CompiledConstructor> _cache = [];

    internal static CompiledConstructor Get(Type type) => _cache.GetOrAdd(type, Compile);

    private static CompiledConstructor Compile(Type type)
    {
        var constructor = GetGreediestConstructor(type);
        return constructor is null ? new(null, []) : CompileConstructor(constructor);
    }

    private static ConstructorInfo? GetGreediestConstructor(Type type) =>
        type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

    private static CompiledConstructor CompileConstructor(ConstructorInfo constructor)
    {
        var parameters = GetParameters(constructor);
        var args = Parameter(typeof(object[]), "args");
        return new(CompileFactory(constructor, parameters, args), parameters);
    }

    private static CompiledParameter[] GetParameters(ConstructorInfo constructor) =>
        [.. constructor.GetParameters().Select(Describe)];

    private static CompiledParameter Describe(ParameterInfo parameter) =>
        new(parameter.Name ?? string.Empty, parameter.ParameterType, parameter.HasDefaultValue, DefaultOf(parameter));

    private static object? DefaultOf(ParameterInfo parameter)
    {
        if (!parameter.HasDefaultValue)
            return null;
        // Reflection reports the default of a struct that is not a primitive as null,
        // so `DateTime stamp = default` has to be turned back into the zeroed value.
        return parameter.DefaultValue
            ?? (parameter.ParameterType.IsValueType && Nullable.GetUnderlyingType(parameter.ParameterType) is null
                ? Activator.CreateInstance(parameter.ParameterType)
                : null);
    }

    private static Func<object[], object> CompileFactory(ConstructorInfo constructor, CompiledParameter[] parameters, ParameterExpression args) =>
        Lambda<Func<object[], object>>(BuildInstantiation(constructor, parameters, args), args).Compile();

    private static UnaryExpression BuildInstantiation(ConstructorInfo constructor, CompiledParameter[] parameters, ParameterExpression args) =>
        Convert(New(constructor, BuildArguments(parameters, args)), typeof(object));

    private static Expression[] BuildArguments(CompiledParameter[] parameters, ParameterExpression args) =>
        [.. parameters.Select((parameter, index) => CastArgument(parameter.Type, index, args))];

    private static UnaryExpression CastArgument(Type targetType, int index, ParameterExpression args) =>
        Convert(ArrayIndex(args, Constant(index)), targetType);
}