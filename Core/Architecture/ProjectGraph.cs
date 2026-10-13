namespace TSpec.Architecture;

/// <summary>
/// The dependencies of projects on projects, and on packages written "P:" and the package name, to
/// assert on with <c>Is()</c>
/// </summary>
[Obsolete(Source.UseProjects)]
public sealed class ProjectGraph
{
    internal ProjectGraph(SourceProjects graph) => Graph = graph;

    internal SourceProjects Graph { get; }

    /// <summary>
    /// The projects and packages the given project references directly, named as the rules see them
    /// </summary>
    /// <param name="project">The project, named as the rules see it</param>
    public IReadOnlyList<string> this[string project] => Graph.ReferencesAsWritten(project);

    /// <summary>
    /// Every project, named as the rules see it
    /// </summary>
    public IReadOnlyList<string> Projects => Graph.NamesButTheSpec;

    /// <summary>
    /// The same dependencies, with projects named relative to the root: under "MyHotel",
    /// "MyHotel.Entry" is "Entry", "MyHotel" itself is "." and a project outside it is "/" and its
    /// name. Packages keep their names
    /// </summary>
    /// <param name="root">The namespace the project names share</param>
    public ProjectGraph Under(string root) => new(Graph.Under(root));
}
