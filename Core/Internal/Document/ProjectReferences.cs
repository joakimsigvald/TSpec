using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace TSpec.Internal.Document;

/// <summary>
/// The projects an assembly references directly, with their versions, read from the deps.json the
/// build wrote beside it. Direct only, and projects only — a package is a library the build did not
/// mark "type": "project". This is where the subject is found and its version read.
/// </summary>
internal sealed class ProjectReferences
{
    private readonly Dictionary<string, string> _versionsByName;

    private ProjectReferences(Dictionary<string, string> versionsByName)
        => _versionsByName = versionsByName;

    internal IReadOnlyCollection<string> Names => _versionsByName.Keys;

    internal bool TryGetVersion(string name, [NotNullWhen(true)] out string? version)
        => _versionsByName.TryGetValue(name, out version);

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
        return new(direct);
    }

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
