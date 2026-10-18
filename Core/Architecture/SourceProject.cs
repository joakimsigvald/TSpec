namespace TSpec.Architecture;

/// <summary>
/// A project of the build, named as the rules see it
/// </summary>
public sealed class SourceProject
{
    internal SourceProject(string name, IReadOnlyList<string> dependencies, IReadOnlyList<string> reaches, string assemblyName)
    {
        Name = name;
        Dependencies = dependencies;
        Reaches = reaches;
        AssemblyName = assemblyName;
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
    /// The projects and packages the project depends on, directly or through other projects, named as
    /// the rules see them
    /// </summary>
    public IReadOnlyList<string> Reaches { get; }

    /// <summary>
    /// The name of the assembly the project builds, in full where <see cref="Name"/> is shortened by a root
    /// </summary>
    public string AssemblyName { get; }

    /// <summary>
    /// The project as a failure names it: its name
    /// </summary>
    public override string ToString() => Name;
}
