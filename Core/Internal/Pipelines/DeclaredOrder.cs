using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// Runs the test methods of a spec class in the order they are declared, a base class's before its
/// subclass's, so that Facts sharing a run read it in the same order every time. A metadata token
/// follows the declaration order within its type.
/// </summary>
internal sealed class DeclaredOrder : ITestMethodOrderer
{
    public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods)
        where TTestMethod : ITestMethod
        => [.. testMethods.OrderBy(it => Position(it))];

    internal static (int Depth, int Token)? Position(ITestMethod? testMethod)
        => testMethod is IXunitTestMethod { Method: var method } ? Position(method) : null;

    private static (int Depth, int Token) Position(MethodInfo method)
        => (BaseTypes(method.DeclaringType).Count(), method.MetadataToken);

    private static IEnumerable<Type> BaseTypes(Type? type)
    {
        for (var baseType = type?.BaseType; baseType is not null; baseType = baseType.BaseType)
            yield return baseType;
    }
}
