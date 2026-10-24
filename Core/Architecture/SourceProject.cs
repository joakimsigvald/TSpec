namespace TSpec.Architecture;

/// <summary>
/// A project of the build, named as the rules see it
/// </summary>
public sealed class SourceProject
{
    private readonly Lazy<IReadOnlyList<string>> _opensInternalsTo;

    internal SourceProject(
        string name, IReadOnlyList<string> dependencies, IReadOnlyList<string> reaches, string assemblyName,
        bool isSpecProject, Func<IReadOnlyList<string>> opensInternalsTo)
    {
        Name = name;
        Dependencies = dependencies;
        Reaches = reaches;
        AssemblyName = assemblyName;
        IsSpecProject = isSpecProject;
        _opensInternalsTo = new(opensInternalsTo);
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
    /// Whether the project is a spec project: it references TSpec, directly or through a project its specs
    /// share
    /// </summary>
    public bool IsSpecProject { get; }

    /// <summary>
    /// The projects of the build its <c>InternalsVisibleTo</c> names, named as the rules see them, in
    /// ordinal order: read from the assembly the build made of it
    /// </summary>
    public IReadOnlyList<string> OpensInternalsTo => _opensInternalsTo.Value;

    /// <summary>
    /// The project as a failure names it: its name
    /// </summary>
    public override string ToString() => Name;
}
