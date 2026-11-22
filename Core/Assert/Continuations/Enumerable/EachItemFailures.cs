using TSpec.Internal.Specification;
using Xunit.Sdk;

namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// The items an assertion on each item fails for, and how the failure of the whole words them: one
/// expectation inline when the assertion expects the same of every item, as <c>it.Is().LessThan(3)</c> does;
/// otherwise each item's own failure below, indented, the item named for its parameter.
/// </summary>
internal sealed class EachItemFailures<TItem>(IReadOnlyList<(TItem Item, XunitException Failure)> failed, string? item, bool alike)
{
    internal static EachItemFailures<TItem> Of(IEnumerable<TItem> items, Action<TItem, int> assert, string? assertExpr)
        => new([.. FailuresOf(items, assert)], assertExpr.ItemParameter(), assertExpr.ExpectsTheSameOfEachItem());

    internal TItem[] Items => [.. failed.Select(at => at.Item)];

    /// <summary>
    /// The expectation every item failed, when the assertion expects the same of each
    /// </summary>
    internal string? Shared => alike && ExpectationsFailed() is [var only] ? only : null;

    /// <summary>
    /// Each item's failure on a line of its own, when the items share no expectation
    /// </summary>
    internal string Below => Shared is null ? string.Concat(Lines().Select(line => $"{Environment.NewLine}  {line}")) : string.Empty;

    private string?[] ExpectationsFailed() => [.. failed.Select(at => ExpectationOf(at.Failure)?.Expectation).Distinct()];

    private IEnumerable<string> Lines() => failed.SelectMany(at => LinesOf(at.Item, at.Failure));

    private string[] LinesOf(TItem value, XunitException failure)
    {
        var lines = failure.Message.Split(Environment.NewLine);
        return ExpectationOf(failure) is { } expectation && Named(expectation, value) is { } first
            ? [first, .. lines[1..]]
            : lines;
    }

    // The item named for its parameter: it is the 3 it was, project.Files is Core's Files.
    private string? Named(FailedExpectation expectation, TItem value)
        => SubjectOf(expectation.Actual, value.FormatValue()) is { } subject
            ? $"Expected {subject} to {expectation.Expectation}{Failure.OutcomeBeside(subject, expectation.Outcome)}"
            : null;

    private string? SubjectOf(string actual, string named)
        => actual == item ? named
        : item is not null && actual.StartsWith($"{item}.", StringComparison.Ordinal) ? $"{named}'s {actual[(item.Length + 1)..]}"
        : null;

    private static FailedExpectation? ExpectationOf(XunitException failure)
        => FailedExpectation.TryRead(failure, out var expectation) ? expectation : null;

    private static IEnumerable<(TItem Item, XunitException Failure)> FailuresOf(IEnumerable<TItem> items, Action<TItem, int> assert)
        => items.Select((value, index) => (Item: value, Failure: FailureOf(() => assert(value, index))))
            .Where(at => at.Failure is not null)
            .Select(at => (at.Item, at.Failure!));

    private static XunitException? FailureOf(Action assert)
    {
        try
        {
            assert();
            return null;
        }
        catch (XunitException failure)
        {
            return failure;
        }
    }
}
