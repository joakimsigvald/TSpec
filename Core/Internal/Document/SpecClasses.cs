using System.Reflection;

namespace TSpec.Internal.Document;

/// <summary>
/// The concrete spec classes of an assembly, and the files they are written in. They say both
/// what a complete run is expected to report and where the spec project sits.
/// </summary>
internal static class SpecClasses
{
    internal static IEnumerable<Type> Of(Assembly assembly) => assembly.GetTypes().Where(IsConcreteSpec);

    /// <summary>
    /// Every file a spec class is written in, as the build's debug information records it. A
    /// class the debug information says nothing about contributes nothing.
    /// </summary>
    internal static IReadOnlyCollection<string> SourcesOf(Assembly assembly)
        => [.. Of(assembly)
            .Select(SourceLocations.Of)
            .Select(at => at?.File)
            .OfType<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)];

    private static bool IsConcreteSpec(Type type)
        => type is { IsAbstract: false, IsGenericTypeDefinition: false } && DerivesFromSpec(type);

    private static bool DerivesFromSpec(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Spec<,>))
                return true;
        return false;
    }
}
