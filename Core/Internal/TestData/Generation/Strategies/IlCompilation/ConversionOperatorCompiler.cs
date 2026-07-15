using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using static System.Linq.Expressions.Expression;

namespace TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

internal static class ConversionOperatorCompiler
{
    private static readonly ParameterExpression _value = Parameter(typeof(object), "value");
    private static readonly ConcurrentDictionary<Type, CompiledOperator> _cache = [];

    internal static CompiledOperator Get(Type type) => _cache.GetOrAdd(type, Compile);

    private static CompiledOperator Compile(Type type)
    {
        var method = GetCastOperator(type);
        return method is null
            ? new(null, null)
            : new(CompileFactory(method), GetParameterType(method));
    }

    private static MethodInfo? GetCastOperator(Type type)
        => type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => (m.Name == "op_Implicit" || m.Name == "op_Explicit") && m.ReturnType == type);

    private static Type GetParameterType(MethodInfo method)
        => method.GetParameters()[0].ParameterType;

    private static Func<object, object> CompileFactory(MethodInfo method)
        => Lambda<Func<object, object>>(BuildInstantiation(method), _value).Compile();

    private static UnaryExpression BuildInstantiation(MethodInfo method)
        => Convert(Call(method, CastArgument(method)), typeof(object));

    private static UnaryExpression CastArgument(MethodInfo method)
        => Convert(_value, GetParameterType(method));
}