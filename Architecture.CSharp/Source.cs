namespace TSpec.Architecture.CSharp;

/// <summary>
/// The source the running spec project was built from
/// </summary>
public static class Source
{
    private static readonly Lazy<SourceProjects> _projects = new(SourceProjects.ReadBuilt);

    /// <summary>
    /// Every project the running spec project was built with, itself too, each with the projects and
    /// packages it references directly, as the build resolved them
    /// </summary>
    public static SourceProjects Projects => _projects.Value;
}
