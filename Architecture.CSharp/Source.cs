using TSpec.Architecture.CSharp.Internal;

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

    /// <summary>
    /// Compiles a piece of code into a source file the way the build's files are compiled, so a rule's
    /// condition can be tried on an example. The code can name the framework's types and the build's
    /// </summary>
    /// <param name="code">The code: "class C { void M() { try { } catch { } } }"</param>
    public static SourceFile Snippet(string code) => Snippets.Of(code);
}
