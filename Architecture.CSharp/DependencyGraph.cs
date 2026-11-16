using System.Collections.ObjectModel;
using TSpec.Architecture.CSharp.Internal;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// Named nodes, each with the names it depends on, such as the folders of a project and the folders
/// their code uses, both in ordinal order. Assert on the dependencies with <c>Is()</c>:
/// <code>folders.Is().Within(folder => folder switch { "Workbench" => ["Chat"], _ => [] });</code>
/// </summary>
/// <param name="node">What a node is, as the table of what each may use names it, such as "Folder"</param>
/// <param name="dependencies">The names each node depends on, by the node's name</param>
public sealed class DependencyGraph(string node, IReadOnlyDictionary<string, IReadOnlyList<string>> dependencies)
    : ReadOnlyDictionary<string, IReadOnlyList<string>>(Sorted(dependencies))
{
    internal IReadOnlyList<string> Headers => [node, "May use"];

    internal IEnumerable<string> DependenciesOutside(IReadOnlyList<Func<string, IEnumerable<string>>> rules)
        => Edges.Where(edge => !Rules.Allow(rules, edge.From, edge.To)).Select(edge => $"{edge.From} -> {edge.To}");

    internal IEnumerable<string> FirstLoop()
        => new LoopSearch(this).TryFind(out var loop) ? [string.Join(" -> ", loop)] : [];

    // What each node may use: a rule is a function of the node, so it is called for every one.
    internal IReadOnlyList<IReadOnlyList<string>> AllowedBy(IReadOnlyList<Func<string, IEnumerable<string>>> rules)
        => [.. Keys.Select(name => AllowedOf(name, rules))];

    // A wildcard names no one node, so only a target written in full draws an arrow.
    internal IReadOnlyList<(string From, string To)> ArrowsAllowedBy(IReadOnlyList<Func<string, IEnumerable<string>>> rules)
        => [.. Keys.SelectMany(from => Keys.Intersect(Rules.TargetsOf(rules, from)).Select(to => (from, to)))];

    private IEnumerable<(string From, string To)> Edges => this.SelectMany(pair => pair.Value.Select(to => (pair.Key, to)));

    private static IReadOnlyList<string> AllowedOf(string name, IReadOnlyList<Func<string, IEnumerable<string>>> rules)
        => [name, Rules.Listed(Rules.TargetsOf(rules, name))];

    private static SortedDictionary<string, IReadOnlyList<string>> Sorted(
        IReadOnlyDictionary<string, IReadOnlyList<string>> dependencies)
        => new(dependencies.ToDictionary(pair => pair.Key, pair => Ordered(pair.Value)), StringComparer.Ordinal);

    private static IReadOnlyList<string> Ordered(IEnumerable<string> names) => [.. names.Order(StringComparer.Ordinal)];
}
