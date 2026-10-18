using System.Collections.Concurrent;

namespace TSpec.Internal.Document;

/// <summary>
/// Gathers what each test reported during a run, for the document written from it. None is open
/// unless a document fixture opened one, so a project that has not opted in pays nothing.
/// </summary>
internal sealed class SpecificationCollector
{
    private readonly ConcurrentDictionary<string, TestResult> _results = new(StringComparer.Ordinal);
    private readonly ConcurrentBag<SpecificationEntry> _entries = [];

    internal static SpecificationCollector? Current { get; private set; }

    internal IReadOnlyCollection<SpecificationEntry> Entries => _entries;

    internal static void Open() => Current = new();

    internal static SpecificationCollector Close()
    {
        var closed = Current ?? throw new InvalidOperationException("No specification collector is open.");
        Current = null;
        return closed;
    }

    // A theory's rows share one requirement, so a row that failed stands whatever the others did.
    internal void Add(string requirement, TestResult result, Func<SpecificationEntry> entry)
    {
        _results.AddOrUpdate(requirement, result, (_, earlier) => earlier is TestResult.Failed ? earlier : result);
        if (result is TestResult.Passed)
            _entries.Add(entry());
    }

    /// <summary>Requirements that were expected but did not complete; empty means the run was complete and green.</summary>
    internal string[] Missing(IReadOnlySet<string> expected)
        => [.. expected.Where(requirement => !IsComplete(requirement)).Order(StringComparer.Ordinal)];

    // A test skipped while it ran states no requirement, but the run is no less complete for it: only
    // the result tells a skip the author chose from a test that never ran.
    private bool IsComplete(string requirement)
        => _results.TryGetValue(requirement, out var result) && result is TestResult.Passed or TestResult.Skipped;
}
