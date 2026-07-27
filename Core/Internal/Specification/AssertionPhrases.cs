namespace TSpec.Internal.Specification;

/// <summary>
/// Describes the assertion steps: Then/That, asserts and their conjunctions,
/// expected exceptions, and mock verifications. Whether an assert starts a
/// sentence or continues one depends on what precedes it, so that decision is
/// carried as <see cref="SpecificationStep.OpensAssertionChain"/> and made
/// while rendering.
/// </summary>
internal class AssertionPhrases(SpecificationRecording recording)
{
    internal void AddThen()
        => recording.Record(() => recording.Add(
            new(StepLayout.SentenceOrPhrase) { Family = StepFamily.Then, OpensAssertionChain = true }));

    internal void AddThat()
        => recording.Record(() => recording.Add(
            new(StepLayout.Word) { Body = "that", OpensAssertionChain = true }));

    internal void AddAssert(string actual, string verb, string? expected)
        => recording.Record(() =>
        {
            // actual is already described text, not source code — never re-parse it
            recording.Add(new(StepLayout.AssertionHead) { Body = actual });
            AddWord(verb.AsWords());
            AddWord(expected.Describe());
        });

    internal void AddAssert(string assertName)
        => recording.Record(() => AddWord(assertName.AsWords()));

    internal void AddAssertConjunction(string conjunction)
        => recording.Record(() => recording.Add(
            new(StepLayout.Phrase) { Body = conjunction, Indentation = 2, OpensAssertionChain = true }));

    internal void AddAssertThrows<TError>(string? binder)
        => recording.Record(() => AddWord($"throws {typeof(TError).Alias()} {binder}".Trim()));

    internal void AddAssertThrows(string expectedExpr)
        => recording.Record(() => AddWord($"throws {expectedExpr.Describe()}"));

    internal void AddAssertDoesNotThrow<TError>()
        => recording.Record(() => AddWord($"does not throw {typeof(TError).Alias()}"));

    internal void AddVerify<TService>(string expressionExpr, string? wasInvokedExpr = null)
        => recording.Record(() =>
        {
            var call = $"{typeof(TService).Alias()}.{expressionExpr.DescribeCall(true)}";
            AddWord(wasInvokedExpr is null ? call : $"{call} {DescribeInvocation(wasInvokedExpr)}");
        });

    internal void AddWasInvoked<TService>(string? wasInvokedExpr)
        => recording.Record(() => AddWord(
            $"{typeof(TService).Alias()} {DescribeInvocation(wasInvokedExpr)}"));

    internal void AddWasInvoked<TService>(string method, string? wasInvokedExpr)
        => recording.Record(() => AddWord(
            $"{typeof(TService).Alias()}.{method} {DescribeInvocation(wasInvokedExpr)}"));

    private void AddWord(string body) => recording.Add(new(StepLayout.Word) { Body = body });

    private static string DescribeInvocation(string? timesExpr)
        => timesExpr.NormalizeTimes() switch
        {
            "" or "AtLeastOnce" => "was invoked",
            "Never" => "was not invoked",
            "Once" => "was invoked once",
            var normalized => $"was invoked {normalized}",
        };
}
