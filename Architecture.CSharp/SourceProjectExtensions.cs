using Microsoft.CodeAnalysis;
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

        /// <summary>
        /// The types the project's files declare, those nested in them however deep too, in the ordinal order
        /// of their full names: a partial type once
        /// </summary>
        public IReadOnlyList<DeclaredType> AllTypes
            => [.. project.Types.SelectMany(type => type.AndNested).OrderBy(type => type.FullName, StringComparer.Ordinal)];

        /// <summary>
        /// The members the project's files declare, file by file in the order of their paths
        /// </summary>
        public IReadOnlyList<DeclaredMember> Members
            => [.. project.Files.SelectMany(file => file.Members)];

        /// <summary>
        /// The nodes of the given kind in the project's files, file by file in the order of their paths
        /// </summary>
        /// <typeparam name="T">The kind of node</typeparam>
        public IReadOnlyList<SourceNode<T>> Nodes<T>() where T : SyntaxNode
            => [.. project.Files.SelectMany(file => file.Nodes<T>())];

        /// <summary>
        /// The folders directly in the project's own that hold its files, in ordinal order, each with the
        /// others it uses: one of its files uses a type a file of the other declares. A file at the project's
        /// root is in no folder. Assert on them with <c>Is().Within(...)</c>
        /// </summary>
        public DependencyGraph Folders
            => ProjectFolders.Of(project);

        /// <summary>
        /// The projects of the build its <c>InternalsVisibleTo</c> names: TSpec's own
        /// <see cref="SourceProject.OpensInternalsTo"/>
        /// </summary>
        [Obsolete("TSpec's own SourceProject.OpensInternalsTo takes its place, read without Roslyn.")]
        public IReadOnlyList<string> OpensInternalsTo
            => project.OpensInternalsTo;
    }
}
