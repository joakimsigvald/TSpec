using System.Text;

namespace TSpec.Internal.Specification;

/// <summary>
/// Marks text a theory row filled in, written into described text where a row's value is injected.
/// A theory parameter is a hole in the specification rather than a value: every row of a theory
/// then states the same claim, which is what lets one requirement stand over a table of rows.
/// </summary>
/// <remarks>
/// The value is kept where the run it belongs to is the only run in sight — the per-test
/// specification, where a failing row has to say what it was given — and dropped by the document,
/// which states the claim once and lists every row's values beneath it. The markers are never part
/// of the text: whatever hands described text to output resolves them one way or the other first.
/// </remarks>
internal static class Hole
{
    private const char Enter = (char)4;
    private const char Exit = (char)5;

    internal static string Mark(string text) => $"{Enter}{text}{Exit}";

    /// The text as the run filled it in.
    internal static string Filled(this string text)
        => Marked(text) ? string.Concat(text.Where(character => !IsMarker(character))) : text;

    /// The text with the holes left open, which every row of a theory writes the same way.
    internal static string Hollow(this string text)
    {
        if (!Marked(text))
            return text;

        StringBuilder kept = new(text.Length);
        var depth = 0;
        foreach (var character in text)
            if (character == Enter)
                depth++;
            else if (character == Exit)
                depth--;
            else if (depth == 0)
                kept.Append(character);
        return kept.ToString();
    }

    internal static IReadOnlyList<SpecificationClause> Filled(
        IReadOnlyList<SpecificationClause> clauses)
        => Resolve(clauses, Filled);

    internal static IReadOnlyList<SpecificationClause> Hollow(
        IReadOnlyList<SpecificationClause> clauses)
        => Resolve(clauses, Hollow);

    private static IReadOnlyList<SpecificationClause> Resolve(
        IReadOnlyList<SpecificationClause> clauses, Func<string, string> resolve)
        => clauses.Any(clause => clause.Steps.Any(step => Marked(step.Body)))
            ? [.. clauses.Select(clause => new SpecificationClause(
                [.. clause.Steps.Select(step => step with { Body = resolve(step.Body) })]))]
            : clauses;

    private static bool Marked(string text) => text.IndexOf(Enter) >= 0;

    private static bool IsMarker(char character) => character is Enter or Exit;
}
