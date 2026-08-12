using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace TSpec.Internal.Document;

/// <summary>
/// The project-to-project references of an assembly, as the build recorded them in its deps.json.
/// Package references are excluded: only libraries the build marked "type": "project" are kept.
/// </summary>
internal sealed class ProjectReferences
{
    private readonly Dictionary<string, string> _versionsByName;
    private readonly Dictionary<string, string[]> _dependenciesByName;
    private readonly HashSet<string> _projects;

    private ProjectReferences(
        Dictionary<string, string> versionsByName,
        Dictionary<string, string[]> dependenciesByName,
        HashSet<string> projects)
    {
        _versionsByName = versionsByName;
        _dependenciesByName = dependenciesByName;
        _projects = projects;
    }

    internal IReadOnlyCollection<string> Names => _versionsByName.Keys;

    internal bool TryGetVersion(string name, [NotNullWhen(true)] out string? version)
        => _versionsByName.TryGetValue(name, out version);

    /// <summary>
    /// Every project built here that the named one is built on, itself included. Walked from the
    /// subject rather than from the spec project, so the test framework stays out of it, and it
    /// stops at packages — those are pinned by version and have no source in the output to read.
    /// </summary>
    internal IReadOnlyCollection<string> ClosureFrom(string name)
    {
        HashSet<string> reached = new(StringComparer.OrdinalIgnoreCase);
        Stack<string> pending = new([name]);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!_projects.Contains(current) || !reached.Add(current))
                continue;
            foreach (var dependency in Dependencies(current))
                pending.Push(dependency);
        }
        return reached;
    }

    private string[] Dependencies(string name)
        => _dependenciesByName.TryGetValue(name, out var dependencies) ? dependencies : [];

    internal static ProjectReferences Read(string baseDirectory, string assemblyName)
    {
        var path = Path.Combine(baseDirectory, $"{assemblyName}.deps.json");
        if (!File.Exists(path))
            throw new SetupFailed(
                $"TSpec could not read the project references of '{assemblyName}': "
                + $"no dependency manifest at '{path}'.");
        return Parse(File.ReadAllText(path), assemblyName);
    }

    internal static ProjectReferences Parse(string depsJson, string assemblyName)
    {
        using var document = JsonDocument.Parse(depsJson);
        var manifest = document.RootElement;
        var direct = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, version) in GetDirectDependencies(manifest, assemblyName))
            if (IsProject(manifest, name, version))
                direct[name] = version;
        var projects = ReadProjects(manifest);
        return new(direct, ReadGraph(manifest, projects), projects);
    }

    /// <summary>
    /// What each project built here depends on. Only the projects: a manifest lists every package
    /// the build resolved, which for a real application is hundreds of libraries the closure would
    /// never walk into anyway.
    /// </summary>
    private static Dictionary<string, string[]> ReadGraph(JsonElement manifest, HashSet<string> projects)
    {
        var graph = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (!manifest.TryGetProperty("targets", out var targets))
            return graph;
        foreach (var target in targets.EnumerateObject())
            foreach (var library in target.Value.EnumerateObject())
            {
                var name = NameOf(library.Name);
                if (!projects.Contains(name))
                    continue;
                graph[name] = library.Value.TryGetProperty("dependencies", out var dependencies)
                    ? [.. dependencies.EnumerateObject().Select(dependency => dependency.Name)]
                    : [];
            }
        return graph;
    }

    private static HashSet<string> ReadProjects(JsonElement manifest)
    {
        var projects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!manifest.TryGetProperty("libraries", out var libraries))
            return projects;
        foreach (var library in libraries.EnumerateObject())
            if (library.Value.TryGetProperty("type", out var type) && type.GetString() == "project")
                projects.Add(NameOf(library.Name));
        return projects;
    }

    /// <summary>A library is keyed "name/version"; the output file is named after the name alone.</summary>
    private static string NameOf(string library)
        => library.IndexOf('/') is var at && at < 0 ? library : library[..at];

    private static Dictionary<string, string> GetDirectDependencies(JsonElement manifest, string assemblyName)
    {
        var dependencies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!manifest.TryGetProperty("targets", out var targets))
            return dependencies;
        var prefix = $"{assemblyName}/";
        foreach (var target in targets.EnumerateObject())
            foreach (var library in target.Value.EnumerateObject())
            {
                if (!library.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!library.Value.TryGetProperty("dependencies", out var direct))
                    continue;
                foreach (var dependency in direct.EnumerateObject())
                    dependencies[dependency.Name] = dependency.Value.GetString() ?? string.Empty;
            }
        return dependencies;
    }

    private static bool IsProject(JsonElement manifest, string name, string version)
        => manifest.TryGetProperty("libraries", out var libraries)
        && libraries.TryGetProperty($"{name}/{version}", out var library)
        && library.TryGetProperty("type", out var type)
        && type.GetString() == "project";
}
