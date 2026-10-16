namespace TSpec.Architecture;

/// <summary>
/// The source the running spec project was built from
/// </summary>
public static class Source
{
    internal const string UseProjects = "Use Source.Projects, which lists the projects themselves: "
        + "graph[\"X\"] is Source.Projects[\"X\"].Dependencies there.";

    private static readonly Lazy<SourceProjects> _projects = new(SourceProjects.ReadBuilt);

    /// <summary>
    /// Every project the running spec project was built with, itself too, each with the projects and
    /// packages it references directly, as the build resolved them
    /// </summary>
    public static SourceProjects Projects => _projects.Value;
}
