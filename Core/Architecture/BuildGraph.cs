using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using TSpec.Internal.Document;

namespace TSpec.Architecture;

/// <summary>
/// The projects of a build by their full names, each with the projects and packages it references
/// directly, and which of them is the running spec project.
/// </summary>
internal sealed class BuildGraph(Dictionary<string, IReadOnlyList<string>> references, string? spec)
{
    private const string Package = "P:";

    internal string? Spec => spec;

    internal IEnumerable<string> Projects => references.Keys;

    // The running spec project references every project to read the build, so no rule is about its references.
    internal IEnumerable<string> ProjectsButTheSpec
        => references.Keys.Where(project => project != spec).Order(StringComparer.Ordinal);

    internal IEnumerable<(string From, string To)> References
        => ProjectsButTheSpec.SelectMany(project => ReferencesOf(project).Order(StringComparer.Ordinal).Select(to => (project, to)));

    // The leading segments every project's name shares, the running spec project aside.
    internal string? SharedNamespace
    {
        get
        {
            var names = ProjectsButTheSpec.Select(name => name.Split('.')).ToArray();
            var shared = string.Join('.', names.FirstOrDefault([])
                .TakeWhile((segment, at) => names.All(name => at < name.Length && name[at] == segment)));
            return shared.Length == 0 ? null : shared;
        }
    }

    internal bool Contains(string project) => references.ContainsKey(project);

    internal IReadOnlyList<string> ReferencesOf(string project)
        => references.TryGetValue(project, out var direct) ? direct : [];

    // A spec project reaches TSpec, directly or through a project its specs share.
    internal bool IsSpecProject(string project) => ReachedFrom(project).Contains($"{Package}TSpec");

    // A spec project is named for the project it specifies and references it: MyHotel.Spec for MyHotel.
    internal bool TryGetTheProjectItsNameExtends(string project, [NotNullWhen(true)] out string? extended)
    {
        extended = ReferencesOf(project).FirstOrDefault(to => IsToTheProjectItsNameExtends((project, to)));
        return extended is not null;
    }

    internal bool IsRedundant((string From, string To) reference)
        => !IsPackage(reference.To) && !IsToTheProjectItsNameExtends(reference) && IsReachedThroughAnother(reference);

    internal static bool IsPackage(string name) => name.StartsWith(Package, StringComparison.Ordinal);

    internal static BuildGraph Parse(string depsJson, string specAssemblyName)
    {
        using var document = JsonDocument.Parse(depsJson);
        var manifest = document.RootElement;
        return new(ProjectReferences.ProjectsIn(manifest)
            .ToDictionary(project => project, project => ReferencesIn(manifest, project)),
            specAssemblyName);
    }

    internal static BuildGraph ReadBuilt()
    {
        var specAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name
            ?? throw new SetupFailed(
                "TSpec could not find the running spec project, so it cannot read the project graph it was built with.");
        return Parse(ProjectReferences.ReadManifest(AppContext.BaseDirectory, specAssemblyName), specAssemblyName);
    }

    private static IReadOnlyList<string> ReferencesIn(JsonElement manifest, string project)
        => [.. ProjectReferences.Of(manifest, project).Names
            .Concat(ProjectReferences.PackagesOf(manifest, project).Select(package => $"{Package}{package}"))
            .Order(StringComparer.Ordinal)];

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
}
