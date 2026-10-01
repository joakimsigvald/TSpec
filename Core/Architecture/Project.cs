namespace TSpec.Architecture;

/// <summary>
/// The projects the running spec project was built with
/// </summary>
public static class Project
{
    private static readonly Lazy<ProjectGraph> _dependencies = new(ProjectGraph.ReadBuilt);

    /// <summary>
    /// Every project the running spec project reaches, but itself, with the projects and packages it
    /// references directly, as its build resolved them
    /// </summary>
    public static ProjectGraph Dependencies => _dependencies.Value;
}
