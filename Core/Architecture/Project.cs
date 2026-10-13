namespace TSpec.Architecture;

/// <summary>
/// The projects the running spec project was built with
/// </summary>
[Obsolete(Source.UseProjects)]
public static class Project
{
    /// <summary>
    /// The projects, their indexer giving a project's direct references and their Projects their names
    /// </summary>
    [Obsolete(Source.UseProjects)]
    public static ProjectGraph Dependencies => new(Source.Projects);
}
