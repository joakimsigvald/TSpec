using System.Collections.Concurrent;

namespace TSpec.Internal.Document;

/// <summary>
/// Gathers passing requirements during a run. Inert unless a <see cref="SpecificationDocument"/>
/// fixture switched it on, so a project that has not opted in pays nothing.
/// </summary>
internal static class SpecificationCollector
{
    private static readonly ConcurrentBag<SpecificationEntry> _entries = [];
    private static int _notPassed;

    internal static bool IsActive { get; set; }

    internal static bool RunWasGreen => Volatile.Read(ref _notPassed) == 0;

    internal static void Record(SpecificationEntry entry) => _entries.Add(entry);

    /// <summary>
    /// A test that did not pass contributes nothing, and its absence must not silently
    /// shorten the document — so the run is marked and the file is left alone.
    /// </summary>
    internal static void RecordNotPassed() => Interlocked.Increment(ref _notPassed);

    internal static IReadOnlyCollection<SpecificationEntry> Entries => _entries;

    /// <summary>Test-only: the collector is process-wide static state.</summary>
    internal static void Reset()
    {
        _entries.Clear();
        Volatile.Write(ref _notPassed, 0);
        IsActive = false;
    }
}
