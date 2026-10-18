using System.Collections.Concurrent;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The declared types of the build that a compiled project's code and signatures name. A project's types are
/// read only once one of them is named.
/// </summary>
internal static class BuildTypes
{
    private static readonly ConcurrentDictionary<string, IReadOnlyDictionary<TypeKey, DeclaredType>> _byAssembly = new();

    internal static IReadOnlyList<DeclaredType> UsedIn(SourceFile file)
        => Of(CompiledProject.Of(Source.Projects[file.Project]).UsedIn(file.Root.SyntaxTree));

    internal static IReadOnlyList<DeclaredType> ExposedBy(DeclaredType type)
    {
        var project = Source.Projects[type.Files[0].Project];
        return Of(CompiledProject.Of(project).NamedBySignaturesOf(KeyOf(type, project)));
    }

    private static IReadOnlyList<DeclaredType> Of(IEnumerable<TypeKey> types)
        => [.. types.Distinct().Select(Find).OfType<DeclaredType>().OrderBy(type => type.FullName, StringComparer.Ordinal)];

    private static DeclaredType? Find(TypeKey type) => _byAssembly.GetOrAdd(type.Assembly, Declared).GetValueOrDefault(type);

    // An assembly no project of the build makes, such as the framework's, declares none of the build's types.
    private static IReadOnlyDictionary<TypeKey, DeclaredType> Declared(string assembly)
        => Source.Projects.FirstOrDefault(project => project.AssemblyName == assembly) is { } project
            ? project.Types.DistinctBy(type => KeyOf(type, project)).ToDictionary(type => KeyOf(type, project))
            : new Dictionary<TypeKey, DeclaredType>();

    private static TypeKey KeyOf(DeclaredType type, SourceProject project)
        => new(project.AssemblyName, type.Namespace, type.Name, type.Arity);
}
