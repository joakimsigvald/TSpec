using System.Reflection;
using System.Text.Json;
using TSpec.Internal.Document;

namespace TSpec.Architecture;

/// <summary>
/// The dependencies of projects on projects, and on packages written "P:" and the package name,
/// to assert on with <c>Is()</c>:
/// <code>Project.Dependencies.Is().Within(project => project switch { "MyHotel.Entry" => ["MyHotel.Contract"], _ => [] });</code>
/// </summary>
public sealed class ProjectGraph
{
    private const string Package = "P:";

    private readonly Dictionary<string, IReadOnlyList<string>> _references;

    // Under only shortens names where the user writes or reads them; checks that compare names, like A.B -> A, see them in full.
    private readonly string? _root;

    internal ProjectGraph() : this([], null) { }

    private ProjectGraph(Dictionary<string, IReadOnlyList<string>> references, string? root)
    {
        _references = references;
        _root = root;
    }

    /// <summary>
    /// The projects and packages the given project references directly, named as the rules see them
    /// </summary>
    /// <param name="project">The project, named as the rules see it</param>
    public IReadOnlyList<string> this[string project]
    {
        get => [.. ReferencesOf(FullName(project)).Select(AsWritten).Order(StringComparer.Ordinal)];
        internal init => _references[project] = value;
    }

    /// <summary>
    /// Every project, named as the rules see it
    /// </summary>
    public IReadOnlyList<string> Projects => [.. _references.Keys.Select(AsWritten).Order(StringComparer.Ordinal)];

    /// <summary>
    /// The same dependencies, with projects named relative to the root: under "MyHotel",
    /// "MyHotel.Entry" is "Entry", "MyHotel" itself is "." and a project outside it is "/" and its
    /// name. Packages keep their names
    /// </summary>
    /// <param name="root">The namespace the project names share</param>
    public ProjectGraph Under(string root) => new(_references, root);

    internal IEnumerable<string> ReferencesOutside(IEnumerable<Func<string, IEnumerable<string>>> rules)
        => References.Where(reference => !rules.Any(rule => IsAllowedBy(rule, reference))).Select(Describe);

    internal IEnumerable<string> RedundantReferences() => References.Where(IsRedundant).Select(Describe);

    internal static ProjectGraph Parse(string depsJson, string specAssemblyName)
    {
        using var document = JsonDocument.Parse(depsJson);
        var manifest = document.RootElement;
        return new(ProjectReferences.ProjectsIn(manifest)
            .Where(project => project != specAssemblyName)
            .ToDictionary(project => project, project => ReferencesOf(manifest, project)), root: null);
    }

    internal static ProjectGraph ReadBuilt()
    {
        var specAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name
            ?? throw new SetupFailed(
                "TSpec could not find the running spec project, so it cannot read the project graph it was built with.");
        return Parse(ProjectReferences.ReadManifest(AppContext.BaseDirectory, specAssemblyName), specAssemblyName);
    }

    private static IReadOnlyList<string> ReferencesOf(JsonElement manifest, string project)
        => [.. ProjectReferences.Of(manifest, project).Names
            .Concat(ProjectReferences.PackagesOf(manifest, project).Select(package => $"{Package}{package}"))
            .Order(StringComparer.Ordinal)];

    private static bool IsPackage(string name) => name.StartsWith(Package, StringComparison.Ordinal);

    private bool IsAllowedBy(Func<string, IEnumerable<string>> rule, (string From, string To) reference)
        => Allows(rule(AsWritten(reference.From)), AsWritten(reference.To));

    private IReadOnlyList<string> ReferencesOf(string project)
        => _references.TryGetValue(project, out var references) ? references : [];

    private string FullName(string written)
        => _root is null || IsPackage(written) ? written
        : written == "." ? _root
        : written.StartsWith('/') ? written[1..]
        : $"{_root}.{written}";

    private string AsWritten(string name)
        => _root is null || IsPackage(name) ? name
        : name == _root ? "."
        : name.StartsWith($"{_root}.", StringComparison.Ordinal) ? name[(_root.Length + 1)..]
        : $"/{name}";

    private static bool Allows(IEnumerable<string> targets, string name) => targets.Any(target => Matches(target, name));

    private static bool Matches(string target, string name)
        => target.EndsWith('*') ? name.StartsWith(target[..^1], StringComparison.Ordinal) : target == name;

    private IEnumerable<(string From, string To)> References
        => _references.Keys.Order(StringComparer.Ordinal)
            .SelectMany(project => ReferencesOf(project).Order(StringComparer.Ordinal).Select(to => (project, to)));

    private bool IsRedundant((string From, string To) reference)
        => !IsPackage(reference.To) && !IsToTheProjectItsNameExtends(reference) && IsReachedThroughAnother(reference);

    // A spec project must reference the project it specifies directly, even when a shared spec project reaches it too.
    private static bool IsToTheProjectItsNameExtends((string From, string To) reference)
        => reference.From.Segment(..^1) == reference.To;

    private bool IsReachedThroughAnother((string From, string To) reference)
        => ReferencesOf(reference.From).Any(other => other != reference.To && ReachedFrom(other).Contains(reference.To));

    private HashSet<string> ReachedFrom(string project)
    {
        HashSet<string> reached = [];
        Stack<string> pending = new([project]);
        while (pending.TryPop(out var current))
            foreach (var next in ReferencesOf(current))
                if (reached.Add(next))
                    pending.Push(next);
        return reached;
    }

    private string Describe((string From, string To) reference) => $"{AsWritten(reference.From)} -> {AsWritten(reference.To)}";
}
