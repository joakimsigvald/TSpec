using System.Reflection;

namespace TSpec.Internal.Document;

/// <summary>
/// The requirements a complete run is expected to report: every non-skipped test method on every
/// concrete <see cref="Spec"/> subclass in the assembly.
/// </summary>
/// <remarks>
/// This is what makes a filtered run detectable. A test that was not run, failed, or threw in its
/// constructor all look the same from here — it simply never reported — so one set comparison
/// covers every way a document could come out short.
/// </remarks>
internal static class ExpectedRequirements
{
    internal static IReadOnlySet<string> Of(Assembly assembly)
        => assembly.GetTypes()
            .Where(IsConcreteSpec)
            .SelectMany(type => TestMethods(type).Select(method => Identity(type, method.Name)))
            .ToHashSet(StringComparer.Ordinal);

    internal static string Identity(Type testClass, string methodName)
        => $"{testClass.FullName}.{methodName}";

    private static bool IsConcreteSpec(Type type)
        => type is { IsAbstract: false, IsGenericTypeDefinition: false } && DerivesFromSpec(type);

    private static bool DerivesFromSpec(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Spec<,>))
                return true;
        return false;
    }

    private static IEnumerable<MethodInfo> TestMethods(Type type)
        => type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttributes<FactAttribute>(inherit: true).Any(NotSkipped));

    private static bool NotSkipped(FactAttribute fact) => string.IsNullOrEmpty(fact.Skip);
}
