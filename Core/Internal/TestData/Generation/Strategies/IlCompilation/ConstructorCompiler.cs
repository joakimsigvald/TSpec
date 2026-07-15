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
        var paramTypes = GetParameterTypes(constructor);
        var args = Parameter(typeof(object[]), "args");
        return new(CompileFactory(constructor, paramTypes, args), paramTypes);
    }

    private static Type[] GetParameterTypes(ConstructorInfo constructor) =>
        [.. constructor.GetParameters().Select(p => p.ParameterType)];

    private static Func<object[], object> CompileFactory(ConstructorInfo constructor, Type[] paramTypes, ParameterExpression args) =>
        Lambda<Func<object[], object>>(BuildInstantiation(constructor, paramTypes, args), args).Compile();

    private static UnaryExpression BuildInstantiation(ConstructorInfo constructor, Type[] paramTypes, ParameterExpression args) =>
        Convert(New(constructor, BuildArguments(paramTypes, args)), typeof(object));

    private static Expression[] BuildArguments(Type[] paramTypes, ParameterExpression args) =>
        [.. paramTypes.Select((type, index) => CastArgument(type, index, args))];

    private static UnaryExpression CastArgument(Type targetType, int index, ParameterExpression args) =>
        Convert(ArrayIndex(args, Constant(index)), targetType);
}