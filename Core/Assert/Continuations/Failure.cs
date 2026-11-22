using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations;

/// <summary>
/// What a failed assertion expected and found. An either holds one for each alternative that failed, and
/// when none holds its message names them all.
/// </summary>
internal sealed record Failure(string Expectation, string Outcome, Xunit.Sdk.XunitException? Inner)
{
    internal static Xunit.Sdk.XunitException Of(string actual, IReadOnlyList<Failure> failures, string? fix = null)
    {
        var failure = new Xunit.Sdk.XunitException(Message(actual, failures) + Remedy(fix), failures[0].Inner);
        ExpectationOf(actual, failures).Attach(failure);
        return failure;
    }

    private static FailedExpectation ExpectationOf(string actual, IReadOnlyList<Failure> failures)
        => new(actual, Expected(failures).StripWrapMarkers().Filled(), failures[0].Outcome.StripWrapMarkers().Filled());

    /// <summary>
    /// What a failure says after its expectation, unless that only repeats the actual as it is named:
    /// "Expected 3 to be less than 2", not "… but it was 3"
    /// </summary>
    internal static string OutcomeBeside(string actual, string outcome)
        => outcome == $" but it was {actual}" ? string.Empty : outcome;

    private static string Message(string actual, IReadOnlyList<Failure> failures)
        => $"Expected {actual} to {Expected(failures)}{OutcomeBeside(actual, failures[0].Outcome)}".StripWrapMarkers().Filled();

    private static string Remedy(string? fix) => fix is null ? string.Empty : $"{Environment.NewLine}Fix: {fix}";

    private static string Expected(IReadOnlyList<Failure> failures)
        => failures is [var only] ? only.Expectation : Alternatives(failures);

    private static string Alternatives(IReadOnlyList<Failure> failures)
        => $"{string.Join(", ", failures.SkipLast(1).Select(failure => failure.Expectation))} or {failures[^1].Expectation}";
}
