namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The files of the build that use each declared type: every file's Uses, turned around once in a run.
/// </summary>
internal static class TypeUsers
{
    private static readonly Lazy<ILookup<DeclaredType, SourceFile>> _byType = new(Read);

    internal static IReadOnlyList<SourceFile> Of(DeclaredType type) => [.. _byType.Value[type]];

    private static ILookup<DeclaredType, SourceFile> Read()
        => Source.Projects.SelectMany(project => project.Files)
            .SelectMany(file => file.Uses, (file, type) => (File: file, Type: type))
            .ToLookup(use => use.Type, use => use.File);
}
