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
        return Of(CompiledProject.Of(project).NamedBySignaturesOf(TypeKey.Of(type, project.AssemblyName)));
    }

    private static IReadOnlyList<DeclaredType> Of(IEnumerable<TypeKey> types)
        => [.. types.Distinct().Select(Find).OfType<DeclaredType>().Distinct()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)];

    // A type no file declares, such as an extension block or what a generator wrote, counts as the one it is in.
    private static DeclaredType? Find(TypeKey type)
        => _byAssembly.GetOrAdd(type.Assembly, Declared).GetValueOrDefault(type)
            ?? (type.IsNested ? Find(type.Owner) : null);

    // An assembly no project of the build makes, such as the framework's, declares none of the build's types.
    private static IReadOnlyDictionary<TypeKey, DeclaredType> Declared(string assembly)
        => Source.Projects.FirstOrDefault(project => project.AssemblyName == assembly) is { } project
            ? project.Types.SelectMany(AndNested)
                .DistinctBy(type => TypeKey.Of(type, assembly))
                .ToDictionary(type => TypeKey.Of(type, assembly))
            : new Dictionary<TypeKey, DeclaredType>();

    private static IEnumerable<DeclaredType> AndNested(DeclaredType type) => [type, .. type.Nested.SelectMany(AndNested)];
}
