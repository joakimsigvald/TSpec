using System.Collections.Concurrent;

namespace TSpec.Internal.Document;

/// <summary>
/// Gathers passing requirements during a run. Inert unless a <see cref="SpecificationDocument"/>
/// fixture switched it on, so a project that has not opted in pays nothing.
/// </summary>
internal static class SpecificationCollector
{
    private static readonly ConcurrentBag<SpecificationEntry> _entries = [];
    private static readonly ConcurrentDictionary<string, byte> _reported = new(StringComparer.Ordinal);

    internal static bool IsActive { get; set; }

    internal static IReadOnlyCollection<SpecificationEntry> Entries => _entries;

    /// <summary>
    /// Only passing tests are recorded. A test that failed, was filtered out, or threw in its
    /// constructor simply never appears — which is what the completeness check detects.
    /// </summary>
    internal static void Record(string identity, SpecificationEntry entry)
    {
        _reported[identity] = 0;
        _entries.Add(entry);
    }

    /// <summary>Requirements that were expected but never reported; empty means the run was complete and green.</summary>
    internal static IReadOnlyCollection<string> Missing(IReadOnlySet<string> expected)
        => expected.Where(requirement => !_reported.ContainsKey(requirement)).Order(StringComparer.Ordinal).ToArray();

    /// <summary>Test-only: the collector is process-wide static state.</summary>
    internal static void Reset()
    {
        _entries.Clear();
        _reported.Clear();
        IsActive = false;
    }
}
