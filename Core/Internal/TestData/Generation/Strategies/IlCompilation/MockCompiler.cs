using Moq;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using static System.Linq.Expressions.Expression;

namespace TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

internal static class MockCompiler
{
    private static readonly ParameterExpression _mock = Parameter(typeof(Mock), "mock");
    private static readonly ConcurrentDictionary<Type, Func<Mock, Type>> _cache = [];

    internal static Type GetMockedType(Mock mock)
        => _cache.GetOrAdd(mock.GetType(), CompileGetter)(mock);

    private static Func<Mock, Type> CompileGetter(Type mockType)
        => Lambda<Func<Mock, Type>>(BuildAccess(mockType), _mock).Compile();

    private static UnaryExpression BuildAccess(Type mockType)
        => Convert(Property(Convert(_mock, mockType), FindProperty(mockType)), typeof(Type));

    private static PropertyInfo FindProperty(Type type)
        => type.GetProperty("MockedType", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException($"Failed to get mocked type property of type {type}");
}