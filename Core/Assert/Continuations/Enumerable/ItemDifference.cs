using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// Items are paired by the assertion's own comparison, not by Equals, or a record holding a list would show
/// as both missing and extra.
/// </summary>
internal sealed record ItemDifference(IReadOnlyList<(int Index, object? Item)> Missing, IReadOnlyList<(int Index, object? Item)> Extra)
{
    private const int LineWidth = 80;

    internal bool IsEmpty => Missing.Count == 0 && Extra.Count == 0;

    internal static ItemDifference Between<TItem>(
        IEnumerable<TItem> expected, IEnumerable<TItem> actual, Action<object?, object?> assertEquivalent)
    {
        var unpaired = Indexed(actual);
        List<(int Index, object? Item)> missing = [];
        foreach (var wanted in Indexed(expected))
            if (!RemovePair(wanted.Item))
                missing.Add(wanted);
        return new(missing, unpaired);

        bool RemovePair(object? wanted)
        {
            var index = unpaired.FindIndex(candidate => AreEquivalent(wanted, candidate.Item, assertEquivalent));
            if (index < 0)
                return false;

            unpaired.RemoveAt(index);
            return true;
        }
    }

    public override string ToString() => Listed("missing", Missing) + Listed("extra", Extra);

    private static List<(int Index, object? Item)> Indexed<TItem>(IEnumerable<TItem> items)
        => [.. items.Select((item, index) => (index, (object?)item))];

    private static bool AreEquivalent(object? expected, object? actual, Action<object?, object?> assertEquivalent)
    {
        try
        {
            assertEquivalent(expected, actual);
            return true;
        }
        catch (Xunit.Sdk.XunitException)
        {
            return false;
        }
    }

    private static string Listed(string label, IReadOnlyList<(int Index, object? Item)> items)
    {
        if (items.Count == 0)
            return string.Empty;

        var entries = items.Select(entry => $"{entry.Index}: {entry.Item.FormatElement()}").ToList();
        var line = $"  {label}: {string.Join(", ", entries)}";
        return line.Length <= LineWidth ? Environment.NewLine + line : OnePerLine(label, entries);
    }

    private static string OnePerLine(string label, IEnumerable<string> entries)
        => string.Join(Environment.NewLine, [string.Empty, $"  {label}:", .. entries.Select(entry => $"    {entry}")]);
}
