using System.Diagnostics.CodeAnalysis;

namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// A sequence that is not a collection may never end, so it is read one item past the collection it is
/// compared with: that item already decides the comparison. Two such sequences cannot be compared.
/// </summary>
internal sealed record ComparedItems<TItem>(
    IEnumerable<TItem> Expected, IEnumerable<TItem> Actual, bool ExpectedIsWhole, bool ActualIsWhole)
{
    internal static bool TryRead(
        IEnumerable<TItem> expected, IEnumerable<TItem> actual, [NotNullWhen(true)] out ComparedItems<TItem>? compared)
    {
        compared = null;
        if (!expected.IsCollection() && !actual.IsCollection())
            return false;

        var (expectedItems, expectedIsWhole) = ReadAgainst(expected, actual);
        var (actualItems, actualIsWhole) = ReadAgainst(actual, expected);
        compared = new(expectedItems, actualItems, expectedIsWhole, actualIsWhole);
        return true;
    }

    // An item is known to be missing only once the actual is read whole, and extra only once the expected is.
    internal ItemDifference Difference(Action<object?, object?> assertEquivalent)
    {
        var difference = ItemDifference.Between(Expected, Actual, assertEquivalent);
        return new(ActualIsWhole ? difference.Missing : [], ExpectedIsWhole ? difference.Extra : []);
    }

    private static (IEnumerable<TItem> Items, bool IsWhole) ReadAgainst(IEnumerable<TItem> items, IEnumerable<TItem> collection)
    {
        if (items.IsCollection())
            return (items, true);

        var count = collection.Count();
        List<TItem> read = [.. items.Take(count + 1)];
        return (read, read.Count <= count);
    }
}
