using TSpec.Architecture.Internal;

namespace TSpec.Architecture;

/// <summary>
/// A project of the build, named as the rules see it
/// </summary>
public sealed class SourceProject
{
    private readonly string _assembly;

    internal SourceProject(string name, IReadOnlyList<string> dependencies, string assembly)
    {
        Name = name;
        Dependencies = dependencies;
        _assembly = assembly;
    }

    /// <summary>
    /// The project's name, as the rules see it
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The projects and packages the project references directly, named as the rules see them
    /// </summary>
    public IReadOnlyList<string> Dependencies { get; }

    /// <summary>
    /// The source files the project compiles from its own folder, in the ordinal order of their paths
    /// in it: what the build generates under obj/, and a file linked in from elsewhere, are left out.
    /// The same files every time, so an extension member can keep what it computes per file
    /// </summary>
    public IReadOnlyList<SourceFile> Files => ProjectSources.Of(Name, _assembly);

    /// <summary>
    /// The project as a failure names it: its name
    /// </summary>
    public override string ToString() => Name;
}
