using System.Collections;
using System.Reflection;
using System.Text.Json;
using TSpec.Internal.Document;

namespace TSpec.Architecture;

/// <summary>
/// The projects of a build, in the ordinal order of their names as the rules see them, each with what
/// it depends on: other projects, and packages written "P:" and the package name. Assert on the
/// dependencies with <c>Is()</c>:
/// <code>Source.Projects.Is().Within(project => project switch { "MyHotel.Entry" => ["MyHotel.Contract"], _ => [] });</code>
/// </summary>
public sealed class SourceProjects : IReadOnlyList<SourceProject>
{
    private const string Package = "P:";

    private readonly Dictionary<string, IReadOnlyList<string>> _references;

    // Under only shortens names where the user writes or reads them; checks that compare names, like A.B -> A, see them in full.
    private readonly string? _root;

    private readonly string? _spec;

    internal SourceProjects(Dictionary<string, IReadOnlyList<string>> references) : this(references, null, null) { }

    private SourceProjects(Dictionary<string, IReadOnlyList<string>> references, string? root, string? spec)
    {
        _references = references;
        _root = root;
        _spec = spec;
    }

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
            return _references.ContainsKey(name)
                ? ProjectOf(name)
                : throw new SetupFailed($"TSpec found no project '{project}' in the build.");
        }
    }

    /// <summary>
    /// The running spec project, which <c>Within</c> and <c>Redundant</c> leave out: it references every
    /// project to read the build
    /// </summary>
    public SourceProject CurrentSpec
        => ProjectOf(_spec ?? throw new InvalidOperationException("Only projects read from a build have a running spec project."));

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

    /// <summary>
    /// The same projects, named relative to the root: under "MyHotel", "MyHotel.Entry" is "Entry",
    /// "MyHotel" itself is "." and a project outside it is "/" and its name. Packages keep their names
    /// </summary>
    /// <param name="root">The namespace the project names share</param>
    public SourceProjects Under(string root) => new(_references, root, _spec);

    // ProjectGraph's view, where the running spec project is no project and a name that is none gives nothing.
    internal IReadOnlyList<string> ReferencesAsWritten(string project)
    {
        var name = FullName(project);
        return name == _spec ? [] : [.. ReferencesOf(name).Select(AsWritten).Order(StringComparer.Ordinal)];
    }

    internal IReadOnlyList<string> NamesButTheSpec
        => [.. _references.Keys.Where(name => name != _spec).Select(AsWritten).Order(StringComparer.Ordinal)];

    internal IEnumerable<string> ReferencesOutside(IEnumerable<Func<string, IEnumerable<string>>> rules)
        => References.Where(reference => !rules.Any(rule => IsAllowedBy(rule, reference))).Select(Describe);

    internal IEnumerable<string> RedundantReferences() => References.Where(IsRedundant).Select(Describe);

    internal static SourceProjects Parse(string depsJson, string specAssemblyName)
    {
        using var document = JsonDocument.Parse(depsJson);
        var manifest = document.RootElement;
        return new(ProjectReferences.ProjectsIn(manifest)
            .ToDictionary(project => project, project => ReferencesOf(manifest, project)),
            root: null, spec: specAssemblyName);
    }

    internal static SourceProjects ReadBuilt()
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

    private IReadOnlyList<SourceProject> Listed
        => [.. _references.Keys.Select(ProjectOf).OrderBy(project => project.Name, StringComparer.Ordinal)];

    private SourceProject ProjectOf(string name)
        => new(AsWritten(name), [.. ReferencesOf(name).Select(AsWritten).Order(StringComparer.Ordinal)],
            Path.Combine(AppContext.BaseDirectory, $"{name}.dll"));

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

    // The running spec project references every project to read the build, so no rule is about its references.
    private IEnumerable<(string From, string To)> References
        => _references.Keys.Where(project => project != _spec).Order(StringComparer.Ordinal)
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
