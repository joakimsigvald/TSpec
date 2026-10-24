using TSpec.Extensibility;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The projects and packages whose types a file's code uses, by the assemblies those types are in.
/// </summary>
internal static class FileDependencies
{
    internal static IReadOnlyList<string> Of(SourceFile file)
        => [.. CompiledProject.UsedIn(file.Model)
            .Select(type => type.Assembly)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(NameOf).OfType<string>()
            .Where(name => name != file.Project)
            .Distinct()
            .Order(StringComparer.Ordinal)];

    // As a target of Within's rules matches a name.
    internal static bool Matches(string name, string dependency)
        => name.EndsWith('*') ? dependency.StartsWith(name[..^1], StringComparison.Ordinal) : dependency == name;

    // An assembly of the framework is neither a project's nor a package's.
    private static string? NameOf(string assembly)
        => ProjectOf(assembly)?.Name ?? (Build.TryFindPackage(assembly, out var package) ? package : null);

    private static SourceProject? ProjectOf(string assembly)
        => Source.Projects.FirstOrDefault(project => string.Equals(project.AssemblyName, assembly, StringComparison.OrdinalIgnoreCase));
}
