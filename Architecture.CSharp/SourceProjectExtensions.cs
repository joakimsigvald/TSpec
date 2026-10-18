using TSpec.Architecture.CSharp.Internal;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// The source files of a project of the build
/// </summary>
public static class SourceProjectExtensions
{
    extension(SourceProject project)
    {
        /// <summary>
        /// The source files the project compiles from its own folder, in the ordinal order of their paths
        /// in it: what the build generates under obj/, and a file linked in from elsewhere, are left out.
        /// The same files every time, so an extension member can keep what it computes per file
        /// </summary>
        public IReadOnlyList<SourceFile> Files
            => ProjectSources.Of(project);

        /// <summary>
        /// The types the project's files declare at their top level, in the ordinal order of their full
        /// names: a partial type once, though several of its files declare it
        /// </summary>
        public IReadOnlyList<DeclaredType> Types
            => [.. project.Files.SelectMany(file => file.Types).Distinct()
                .OrderBy(type => type.FullName, StringComparer.Ordinal)];
    }
}
