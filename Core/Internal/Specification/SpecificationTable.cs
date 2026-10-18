namespace TSpec.Internal.Specification;

/// What an assertion states as a table rather than a sentence, such as what each project may reference.
/// Arrows, where given, are drawn above the table: from one row's name to another's.
internal sealed record SpecificationTable(
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    IReadOnlyList<(string From, string To)>? Arrows = null)
{
    internal bool Matches(SpecificationTable? other)
        => other is not null
            && Headers.SequenceEqual(other.Headers)
            && Rows.Count == other.Rows.Count
            && Rows.Zip(other.Rows).All(pair => pair.First.SequenceEqual(pair.Second));
}
