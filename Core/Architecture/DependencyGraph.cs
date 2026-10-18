using System.Collections.ObjectModel;
using TSpec.Internal.Specification;

namespace TSpec.Architecture;

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
    internal IEnumerable<string> DependenciesOutside(IReadOnlyList<Func<string, IEnumerable<string>>> rules)
        => Edges.Where(edge => !Rules.Allow(rules, edge.From, edge.To)).Select(edge => $"{edge.From} -> {edge.To}");

    // What each node may use: a rule is a function of the node, so it is called for every one.
    internal SpecificationTable AllowedBy(IReadOnlyList<Func<string, IEnumerable<string>>> rules)
        => new([node, "May use"], [.. Keys.Select(name => AllowedOf(name, rules))], ArrowsAllowedBy(rules));

    private IEnumerable<(string From, string To)> Edges => this.SelectMany(pair => pair.Value.Select(to => (pair.Key, to)));

    // A wildcard names no one node, so only a target written in full draws an arrow.
    private IReadOnlyList<(string From, string To)> ArrowsAllowedBy(IReadOnlyList<Func<string, IEnumerable<string>>> rules)
        => [.. Keys.SelectMany(from => Keys.Intersect(Rules.TargetsOf(rules, from)).Select(to => (from, to)))];

    private static IReadOnlyList<string> AllowedOf(string name, IReadOnlyList<Func<string, IEnumerable<string>>> rules)
        => [name, Rules.Listed(Rules.TargetsOf(rules, name))];

    private static SortedDictionary<string, IReadOnlyList<string>> Sorted(
        IReadOnlyDictionary<string, IReadOnlyList<string>> dependencies)
        => new(dependencies.ToDictionary(pair => pair.Key, pair => Ordered(pair.Value)), StringComparer.Ordinal);

    private static IReadOnlyList<string> Ordered(IEnumerable<string> names) => [.. names.Order(StringComparer.Ordinal)];
}
