namespace TSpec.Internal.Specification;

/// <summary>
/// Describes the assertion steps: Then/That, asserts and their conjunctions,
/// expected exceptions, and mock verifications. <c>Then</c> and the conjunctions
/// head a statement; everything else appends to the one in hand, so which claim a
/// step belongs to is settled when it is recorded rather than inferred later.
/// </summary>
internal class AssertionPhrases(SpecificationRecording recording)
{
    private bool _continuesClaim;

    internal void AddThen()
        => recording.Record(() => recording.Add(new(StepLayout.SentenceOrPhrase)
        {
            Family = StepFamily.Then,
            Introduces = true,
        }));

    internal void AddThat()
        => recording.Record(() => recording.Add(new(StepLayout.Word)
        {
            Body = "that",
            Introduces = true,
        }));

    internal void AddAssert(string actual, string verb, string? expected)
        => recording.Record(() =>
        {
            // actual is already described text, not source code — never re-parse it. After a
            // conjunction the claim goes on about the same actual, which is not said twice.
            recording.Claim(new(StepLayout.Word) { Body = _continuesClaim ? string.Empty : actual });
            _continuesClaim = false;
            AddWord(verb.AsWords());
            AddWord(expected.Describe());
        });

    internal void AddAssert(string assertName)
        => recording.Record(() => AddWord(assertName.AsWords()));

    internal void AddAssertConjunction(string conjunction)
        => recording.Record(() =>
        {
            recording.Add(new(StepLayout.Phrase)
            {
                Body = conjunction,
                Indentation = 2,
                Introduces = true,
            });
            _continuesClaim = true;
        });

    internal void AddAssertThrows<TError>(string? binder)
        => recording.Record(() => recording.Add(new(StepLayout.Word)
        {
            // A binder hands off to the condition that follows, which is the same claim continued.
            Body = $"throws {typeof(TError).Alias()} {binder}".Trim(),
            Introduces = binder is not null,
        }));

    internal void AddAssertThrows(string expectedExpr)
        => recording.Record(() => AddWord($"throws {expectedExpr.Describe()}"));

    internal void AddVerify<TService>(string mock, string expressionExpr, string? wasInvokedExpr)
        => recording.Record(() =>
        {
            var call = expressionExpr.DescribeMockCallOn<TService>(mock);
            AddWord(wasInvokedExpr is null ? call : $"{call} {DescribeInvocation(wasInvokedExpr)}");
        });

    internal void AddWasInvoked(string mock, string? wasInvokedExpr)
        => recording.Record(() => AddWord(
            $"{mock} {DescribeInvocation(wasInvokedExpr)}"));

    internal void AddWasInvoked(string mock, string method, string? wasInvokedExpr)
        => recording.Record(() => AddWord(
            $"{mock}.{method} {DescribeInvocation(wasInvokedExpr)}"));

    private void AddWord(string body)
        => recording.Add(new(StepLayout.Word) { Body = body });

    private static string DescribeInvocation(string? timesExpr)
        => timesExpr.NormalizeTimes() switch
        {
            "" or "AtLeastOnce" => "was invoked",
            "Never" => "was not invoked",
            "Once" => "was invoked once",
            var normalized => $"was invoked {normalized}",
        };
}
