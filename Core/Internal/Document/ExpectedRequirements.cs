using System.Reflection;

namespace TSpec.Internal.Document;

/// <summary>
/// The requirements a complete run is expected to report: every test method that is neither skipped
/// nor explicit, on every concrete <see cref="Spec"/> subclass in the assembly.
/// </summary>
/// <remarks>
/// This is what makes a filtered run detectable. A test that was not run, failed, or threw in its
/// constructor all look the same from here — it simply never reported — so one set comparison
/// covers every way a document could come out short.
/// </remarks>
internal static class ExpectedRequirements
{
    internal static IReadOnlySet<string> Of(Assembly assembly)
        => SpecClasses.Of(assembly)
            .SelectMany(type => TestMethods(type).Select(method => Identity(type, method.Name)))
            .ToHashSet(StringComparer.Ordinal);

    internal static string Identity(Type testClass, string methodName)
        => $"{testClass.FullName}.{methodName}";

    private static IEnumerable<MethodInfo> TestMethods(Type type)
        => type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttributes<FactAttribute>(inherit: true).Any(RunsByDefault));

    private static bool RunsByDefault(FactAttribute fact) => string.IsNullOrEmpty(fact.Skip) && !fact.Explicit;
}
