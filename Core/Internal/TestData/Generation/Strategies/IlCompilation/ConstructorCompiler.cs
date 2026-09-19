using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

internal static class ConstructorCompiler
{
    private static readonly ConcurrentDictionary<Type, CompiledConstructor> _cache = [];

    private static readonly ConcurrentDictionary<Type, CompiledParameter[]> _mockCache = [];

    internal static CompiledConstructor Get(Type type) => _cache.GetOrAdd(type, Compile);

    /// <summary>
    /// The parameters of the constructor a mock of the type is made with: none where it has a
    /// parameterless one, which a class built to be mocked keeps for that; else those of its greediest,
    /// a protected one included, as the mock is a subclass.
    /// </summary>
    internal static CompiledParameter[] GetForMock(Type type) => _mockCache.GetOrAdd(type, DescribeForMock);

    private static CompiledConstructor Compile(Type type)
    {
        var constructor = GetGreediestConstructor(type.GetConstructors());
        return constructor is null ? new(null, []) : CompileConstructor(constructor);
    }

    private static CompiledParameter[] DescribeForMock(Type type)
    {
        var constructors = type
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(constructor => constructor.IsPublic || constructor.IsFamily || constructor.IsFamilyOrAssembly)
            .ToArray();
        if (constructors.Any(constructor => constructor.GetParameters().Length == 0))
            return [];

        return GetGreediestConstructor(constructors) is { } greediest ? GetParameters(greediest) : [];
    }

    private static ConstructorInfo? GetGreediestConstructor(ConstructorInfo[] constructors) =>
        constructors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

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