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

    internal ProjectGraph() : this([]) { }

    private ProjectGraph(Dictionary<string, IReadOnlyList<string>> references) => _references = references;

    internal IReadOnlyList<string> this[string project]
    {
        get => _references.TryGetValue(project, out var references) ? references : [];
        init => _references[project] = value;
    }

    internal IReadOnlyList<string> Projects => [.. _references.Keys.Order(StringComparer.Ordinal)];

    /// <summary>
    /// The same dependencies, with projects named relative to the root: under "MyHotel",
    /// "MyHotel.Entry" is "Entry", "MyHotel" itself is "." and a project outside it is "/" and its
    /// name. Packages keep their names
    /// </summary>
    /// <param name="root">The namespace the project names share</param>
    public ProjectGraph Under(string root)
        => new(_references.ToDictionary(
            project => Relative(project.Key, root),
            project => (IReadOnlyList<string>)[.. project.Value.Select(to => Relative(to, root)).Order(StringComparer.Ordinal)]));

    private static string Relative(string name, string root)
        => IsPackage(name) ? name
        : name == root ? "."
        : name.StartsWith($"{root}.", StringComparison.Ordinal) ? name[(root.Length + 1)..]
        : $"/{name}";

    internal IEnumerable<string> ReferencesOutside(IEnumerable<Func<string, IEnumerable<string>>> rules)
        => References.Where(reference => !rules.Any(allowed => Allows(allowed(reference.From), reference.To)))
            .Select(Describe);

    internal IEnumerable<string> RedundantReferences()
        => References.Where(reference => !IsPackage(reference.To) && IsReachedThroughAnother(reference))
            .Select(Describe);

    internal static ProjectGraph Parse(string depsJson, string specAssemblyName)
    {
        using var document = JsonDocument.Parse(depsJson);
        var manifest = document.RootElement;
        return new(ProjectReferences.ProjectsIn(manifest)
            .Where(project => project != specAssemblyName)
            .ToDictionary(project => project, project => ReferencesOf(manifest, project)));
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

    private static bool Allows(IEnumerable<string> targets, string name) => targets.Any(target => Matches(target, name));

    private static bool Matches(string target, string name)
        => target.EndsWith('*') ? name.StartsWith(target[..^1], StringComparison.Ordinal) : target == name;

    private IEnumerable<(string From, string To)> References
        => Projects.SelectMany(project => this[project].Order(StringComparer.Ordinal).Select(to => (project, to)));

    private bool IsReachedThroughAnother((string From, string To) reference)
        => this[reference.From].Any(other => other != reference.To && ReachedFrom(other).Contains(reference.To));

    private HashSet<string> ReachedFrom(string project)
    {
        HashSet<string> reached = [];
        Stack<string> pending = new([project]);
        while (pending.TryPop(out var current))
            foreach (var next in this[current])
                if (reached.Add(next))
                    pending.Push(next);
        return reached;
    }

    private static string Describe((string From, string To) reference) => $"{reference.From} -> {reference.To}";
}
