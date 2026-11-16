using System.Collections;
using TSpec.Architecture.CSharp.Internal;
using TSpec.Extensibility;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// The projects of a build, in the ordinal order of their names as the rules see them, each with what
/// it depends on: other projects, and packages written "P:" and the package name. Assert on the
/// dependencies with <c>Is()</c>:
/// <code>Source.Projects.Is().Within(project => project switch { "Entry" => ["Contract"], _ => [] });</code>
/// </summary>
public sealed class SourceProjects : IReadOnlyList<SourceProject>
{
    internal SourceProjects(Dictionary<string, IReadOnlyList<string>> references, string? spec = null)
    {
        Graph = new(references, spec);
        Root = Graph.TryGetRoot(out var root) ? root : null;
    }

    internal BuildGraph Graph { get; }

    // The root only shortens names where the user writes or reads them; checks that compare names, like A.B -> A, see them in full.
    internal string? Root { get; }

    /// <summary>
    /// The given project
    /// </summary>
    /// <param name="project">The project, named as the rules see it</param>
    public SourceProject this[string project]
    {
        get
        {
            var name = FullName(project);
            // A misspelt project must not pass every rule on it by having nothing.
            return Graph.Contains(name)
                ? ProjectOf(name)
                : throw new SetupFailed($"TSpec found no project '{project}' in the build.");
        }
    }

    /// <summary>
    /// The running spec project, which <c>Within</c> and <c>NonRedundant</c> leave out: it references every
    /// project to read the build
    /// </summary>
    public SourceProject CurrentSpec
        => ProjectOf(Graph.Spec ?? throw new InvalidOperationException("Only projects read from a build have a running spec project."));

    /// <summary>
    /// The project at the given position
    /// </summary>
    /// <param name="index">The position, in the ordinal order of the projects' names</param>
    public SourceProject this[int index] => Listed[index];

    /// <summary>
    /// How many projects there are
    /// </summary>
    public int Count => Listed.Count;

    /// <summary>
    /// The projects, in the ordinal order of their names as the rules see them
    /// </summary>
    public IEnumerator<SourceProject> GetEnumerator() => Listed.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal IReadOnlyList<string> Headers => ["Project", "May reference", "May use"];

    internal IEnumerable<string> ReferencesOutside(IEnumerable<Func<string, IEnumerable<string>>> rules)
        => Graph.References.Where(reference => !IsAllowedBy(rules, reference)).Select(Describe);

    internal IEnumerable<string> RedundantReferences() => Graph.References.Where(Graph.IsRedundant).Select(Describe);

    // What each project may reference and use: a rule is a function of the project, so it is called for every one.
    internal IReadOnlyList<IReadOnlyList<string>> AllowedBy(IReadOnlyList<Func<string, IEnumerable<string>>> rules)
        => [.. NamesButTheSpec.Select(project => AllowedOf(project, rules))];

    internal static SourceProjects ReadBuilt() => new(new(Build.References), Build.SpecProject);

    private IReadOnlyList<string> NamesButTheSpec => [.. Graph.ProjectsButTheSpec.Select(AsWritten).Order(StringComparer.Ordinal)];

    // To the rules the root is "." and its spec project "Spec", which read as nothing on a page.
    private string Shown(string name)
        => Graph.TryGetTheProjectItsNameExtends(name, out var extended) ? $"{Shown(extended)}.{name.Segment(^1)}"
        : name == Root ? name.Segment(^1)
        : AsWritten(name);

    private string AsWritten(string name)
        => Root is null || BuildGraph.IsPackage(name) ? name
        : name == Root ? "."
        : name.StartsWith($"{Root}.", StringComparison.Ordinal) ? name[(Root.Length + 1)..]
        : $"/{name}";

    private string FullName(string written)
        => Root is null || BuildGraph.IsPackage(written) ? written
        : written == "." ? Root
        : written.StartsWith('/') ? written[1..]
        : $"{Root}.{written}";

    private IReadOnlyList<SourceProject> Listed
        => [.. Graph.Projects.Select(ProjectOf).OrderBy(project => project.Name, StringComparer.Ordinal)];

    private SourceProject ProjectOf(string name)
        => new(AsWritten(name), WrittenReferencesOf(name), WrittenReachOf(name), name,
            Graph.IsSpecProject(name), () => WrittenFriendsOf(name));

    // A friend outside the build, such as a mocking library's proxy assembly, is no project of it.
    private IReadOnlyList<string> WrittenFriendsOf(string name)
        => [.. BuiltAssembly.FriendsOf(name).Where(Graph.Contains).Select(AsWritten).Order(StringComparer.Ordinal)];

    private IReadOnlyList<string> WrittenReferencesOf(string name)
        => [.. Graph.ReferencesOf(name).Select(AsWritten).Order(StringComparer.Ordinal)];

    private IReadOnlyList<string> WrittenReachOf(string name)
        => [.. Graph.ReachedFrom(name).Select(AsWritten).Order(StringComparer.Ordinal)];

    private IReadOnlyList<string> AllowedOf(string project, IReadOnlyList<Func<string, IEnumerable<string>>> rules)
    {
        var targets = Rules.TargetsOf(rules, project);
        return [
            Shown(FullName(project)),
            Rules.Listed(targets.Where(target => !BuildGraph.IsPackage(target)).Select(ShownTarget)),
            Rules.Listed(targets.Where(BuildGraph.IsPackage).Select(BuildGraph.PackageName))];
    }

    // A wildcard names no one project, so it stays as the rule wrote it.
    private string ShownTarget(string target) => target.EndsWith('*') ? target : Shown(FullName(target));

    private bool IsAllowedBy(IEnumerable<Func<string, IEnumerable<string>>> rules, (string From, string To) reference)
        => Rules.Allow(rules, AsWritten(reference.From), AsWritten(reference.To));

    private string Describe((string From, string To) reference) => $"{AsWritten(reference.From)} -> {AsWritten(reference.To)}";
}
