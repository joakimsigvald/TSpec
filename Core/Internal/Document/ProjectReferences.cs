using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace TSpec.Internal.Document;

/// <summary>
/// The projects an assembly references directly, with their versions, read from the deps.json the
/// build wrote beside it. Direct only, and projects only — a package is a library the build marked
/// "type": "package". This is where the subject is found and its version read, and where the
/// project graph, with each project's packages, is read from.
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
        => Parse(ReadManifest(baseDirectory, assemblyName), assemblyName);

    internal static string ReadManifest(string baseDirectory, string assemblyName)
    {
        var path = Path.Combine(baseDirectory, $"{assemblyName}.deps.json");
        if (!File.Exists(path))
            throw new SetupFailed(
                $"TSpec could not read the project references of '{assemblyName}': "
                + $"no dependency manifest at '{path}'.");
        return File.ReadAllText(path);
    }

    internal static ProjectReferences Parse(string depsJson, string assemblyName)
    {
        using var document = JsonDocument.Parse(depsJson);
        return Of(document.RootElement, assemblyName);
    }

    internal static ProjectReferences Of(JsonElement manifest, string assemblyName)
    {
        var direct = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, version) in GetDirectDependencies(manifest, assemblyName))
            if (Is(manifest, name, version, "project"))
                direct[name] = version;
        return new(direct);
    }

    internal static IReadOnlyList<string> PackagesOf(JsonElement manifest, string assemblyName)
        => [.. GetDirectDependencies(manifest, assemblyName)
            .Where(dependency => Is(manifest, dependency.Key, dependency.Value, "package"))
            .Select(dependency => dependency.Key)];

    internal static IReadOnlyList<string> ProjectsIn(JsonElement manifest)
        => manifest.TryGetProperty("libraries", out var libraries)
            ? [.. libraries.EnumerateObject()
                .Where(library => Is(library.Value, "project"))
                .Select(library => library.Name[..library.Name.IndexOf('/')])]
            : [];

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

    private static bool Is(JsonElement manifest, string name, string version, string libraryType)
        => manifest.TryGetProperty("libraries", out var libraries)
        && libraries.TryGetProperty($"{name}/{version}", out var library)
        && Is(library, libraryType);

    private static bool Is(JsonElement library, string libraryType)
        => library.TryGetProperty("type", out var type) && type.GetString() == libraryType;
}
