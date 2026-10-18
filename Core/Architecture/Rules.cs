namespace TSpec.Architecture;

/// What the rules of Within allow, the same for every graph: a rule is a function from a name to the
/// targets it may depend on, rules add up, and a target ending in "*" allows every name that begins with
/// what precedes it.
internal static class Rules
{
    internal static bool Allow(IEnumerable<Func<string, IEnumerable<string>>> rules, string from, string to)
        => rules.Any(rule => rule(from).Any(target => Matches(target, to)));

    internal static IReadOnlyList<string> TargetsOf(IEnumerable<Func<string, IEnumerable<string>>> rules, string name)
        => [.. rules.SelectMany(rule => rule(name)).Distinct()];

    // Beside "*" for any, an empty cell would read as undecided rather than as nothing.
    internal static string Listed(IEnumerable<string> names) => names.Any() ? string.Join(", ", names) : "-";

    private static bool Matches(string target, string name)
        => target.EndsWith('*') ? name.StartsWith(target[..^1], StringComparison.Ordinal) : target == name;
}
