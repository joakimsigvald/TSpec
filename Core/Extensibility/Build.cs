using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using TSpec.Architecture;
using TSpec.Internal.Document;

namespace TSpec.Extensibility;

/// <summary>
/// The build the running spec project was made from, for an extension package that reads more of it than
/// TSpec does
/// </summary>
public static class Build
{
    /// <summary>
    /// Opens the debug information the project's assembly was built with, beside it or embedded in it
    /// </summary>
    /// <param name="project">A project of the build</param>
    /// <param name="provider">The debug information, for the caller to dispose</param>
    /// <returns>Whether the build output holds debug information for the project</returns>
    public static bool TryOpenDebugInformation(
        SourceProject project, [NotNullWhen(true)] out MetadataReaderProvider? provider)
        => SourceLocations.TryOpenDebugInformation(
            Path.Combine(AppContext.BaseDirectory, $"{project.AssemblyName}.dll"), out provider);

    /// <summary>
    /// Finds the folder of the project most of the source files belong to: the nearest above them that holds
    /// a project file
    /// </summary>
    /// <param name="sources">Source files as the debug information names them</param>
    /// <param name="folder">The project's folder</param>
    /// <returns>Whether the source files are on disk under a project</returns>
    public static bool TryFindProjectFolder(IReadOnlyCollection<string> sources, [NotNullWhen(true)] out string? folder)
    {
        folder = ProjectDirectory.Holding(sources);
        return folder is not null;
    }

    /// <summary>
    /// Finds the package of the build an assembly comes from, named as the rules see it: "P:Dapper"
    /// </summary>
    /// <param name="assemblyName">The assembly's name: "Dapper"</param>
    /// <param name="package">The package</param>
    /// <returns>Whether a package brings the assembly: a project's or the framework's comes from none</returns>
    public static bool TryFindPackage(string assemblyName, [NotNullWhen(true)] out string? package)
        => Source.Projects.Graph.TryGetPackageOf(assemblyName, out package);
}
