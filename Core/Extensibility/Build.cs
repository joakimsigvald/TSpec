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
    /// The projects of the build by their assembly names, the running spec project's too, each with the
    /// projects and packages it references directly: a project by its assembly name, a package "P:" and its
    /// name. A reference that produces no assembly is none
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> References => BuildGraph.Built.ReferencesByProject;

    /// <summary>
    /// The assembly name of the running spec project
    /// </summary>
    public static string SpecProject => BuildGraph.Built.Spec!;

    /// <summary>
    /// Opens the debug information an assembly of the build was built with, beside it or embedded in it
    /// </summary>
    /// <param name="assemblyName">The assembly's name: a project's assembly name</param>
    /// <param name="provider">The debug information, for the caller to dispose</param>
    /// <returns>Whether the build output holds debug information for the assembly</returns>
    public static bool TryOpenDebugInformation(
        string assemblyName, [NotNullWhen(true)] out MetadataReaderProvider? provider)
        => SourceLocations.TryOpenDebugInformation(
            Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.dll"), out provider);

    /// <summary>
    /// Opens the debug information the project's assembly was built with, beside it or embedded in it
    /// </summary>
    /// <param name="project">A project of the build</param>
    /// <param name="provider">The debug information, for the caller to dispose</param>
    /// <returns>Whether the build output holds debug information for the project</returns>
    [Obsolete("SourceProject moved to TSpec.Architecture.CSharp: pass the project's AssemblyName.")]
    public static bool TryOpenDebugInformation(
        SourceProject project, [NotNullWhen(true)] out MetadataReaderProvider? provider)
        => TryOpenDebugInformation(project.AssemblyName, out provider);

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
        => BuildGraph.Built.TryGetPackageOf(assemblyName, out package);
}
