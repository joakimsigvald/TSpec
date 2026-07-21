namespace TSpec.Internal.Specification;

/// <summary>
/// Phrases for the assertion steps: Then/That, asserts and their
/// conjunctions, expected exceptions, and mock verifications.
/// </summary>
internal class AssertionPhrases(SpecificationRecording recording, TextBuilder textBuilder)
{
    private int _thenCount;
    private bool _isChainOfAssertions;

    internal void AddThen()
        => recording.Record(() =>
        {
            _isChainOfAssertions = true;
            textBuilder.AddPhraseOrSentence(NextThenWord());
        });

    internal void AddThat()
        => recording.Record(() =>
        {
            _isChainOfAssertions = true;
            textBuilder.AddWord("that");
        });

    internal void AddAssert(string actual, string verb, string? expected)
        => recording.Record(() =>
        {
            // actual is already described text, not source code — never re-parse it
            if (_isChainOfAssertions)
                textBuilder.AddWord(actual);
            else
                textBuilder.AddSentence(actual);
            textBuilder.AddWord(verb.AsWords());
            textBuilder.AddWord(expected.ParseValue());
            _isChainOfAssertions = false;
        });

    internal void AddAssert(string assertName)
        => recording.Record(() => textBuilder.AddWord(assertName.AsWords()));

    internal void AddAssertConjunction(string conjunction)
        => recording.Record(() =>
        {
            _isChainOfAssertions = true;
            textBuilder.AddPhrase(conjunction, 2);
        });

    internal void AddAssertThrows<TError>(string? binder)
        => recording.Record(() => textBuilder.AddWord($"throws {typeof(TError).Alias()} {binder}".Trim()));

    internal void AddAssertThrows(string expectedExpr)
        => recording.Record(() => textBuilder.AddWord($"throws {expectedExpr.ParseValue()}"));

    internal void AddAssertDoesNotThrow<TError>()
        => recording.Record(() => textBuilder.AddWord($"does not throw {typeof(TError).Alias()}"));

    internal void AddVerify<TService>(string expressionExpr)
        => recording.Record(() => textBuilder.AddWord($"{typeof(TService).Alias()}.{expressionExpr.ParseCall(true)}"));

    internal void AddWasInvoked<TService>(string? timesExpr)
        => recording.Record(() => textBuilder.AddWord($"{typeof(TService).Alias()} {DescribeInvocation(timesExpr)}"));

    private static string DescribeInvocation(string? timesExpr)
        => timesExpr.NormalizeTimes() switch
        {
            "" => "was invoked",
            "Never" => "was not invoked",
            "Once" => "was invoked once",
            var normalized => $"was invoked {normalized}",
        };

    private string NextThenWord() => 0 == _thenCount++ ? "Then" : "and";
}