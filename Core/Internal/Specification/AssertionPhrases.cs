namespace TSpec.Internal.Specification;

/// <summary>
/// Describes the assertion steps: Then/That, asserts and their conjunctions,
/// expected exceptions, and mock verifications. <c>Then</c> and the conjunctions
/// head a statement; everything else appends to the one in hand, so which claim a
/// step belongs to is settled when it is recorded rather than inferred later.
/// </summary>
internal class AssertionPhrases(SpecificationRecording recording)
{
    private const string Comma = ",";

    // Said with the assertion it introduces, so an either after or can still turn that or into a comma.
    private string? _conjunction;

    // The alternatives of an open either, said once it ends, since a Fix ending it is left out.
    private List<Claim>? _alternatives;

    // A claim after one said of each item is about the whole again, which a conjunction indented under the items
    // would not show.
    private bool _afterEachItem;

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

    internal void AddAssert(string actual, string verb, string? expected, bool ofEachItem = false)
        => recording.Record(() =>
        {
            var claim = new Claim(actual, verb, expected, _conjunction, ofEachItem);
            _conjunction = null;
            if (_alternatives is null)
                Say(claim);
            else
                AddAlternative(claim);
        });

    internal void AddAssert(string assertName)
        => recording.Record(() => AddWord(assertName.AsWords()));

    /// <summary>
    /// The alternatives of an either are listed with commas, "either A, B or C": _ is written as one.
    /// </summary>
    internal void AddAssertConjunction(string conjunction)
        => recording.Record(() => _conjunction = conjunction == "_" ? Comma : conjunction);

    internal void OpenEither() => recording.Record(() => _alternatives ??= []);

    internal void AddFix()
        => recording.Record(() =>
        {
            _conjunction = null;
            if (_alternatives is not null)
                SayAlternatives(WithoutFix(_alternatives));
        });

    // The alternative after or is the last.
    private void AddAlternative(Claim alternative)
    {
        _alternatives!.Add(alternative);
        if (alternative.Conjunction == "or")
            SayAlternatives(_alternatives);
    }

    // Without its Fix, the last of several alternatives follows or.
    private static List<Claim> WithoutFix(List<Claim> alternatives)
        => alternatives is [_]
            ? alternatives
            : [.. alternatives.SkipLast(1), alternatives[^1] with { Conjunction = "or" }];

    private void SayAlternatives(List<Claim> alternatives)
    {
        _alternatives = null;
        foreach (var alternative in alternatives)
            Say(alternative);
    }

    private void Say(Claim claim)
    {
        var resumesTheWhole = claim.Conjunction is not null && _afterEachItem;
        Say(claim.Conjunction, indentation: resumesTheWhole ? 1 : 2);
        _afterEachItem = claim.OfEachItem;
        if (claim.OfEachItem)
            SayOfEachItem(claim);
        else
            SayOnce(claim, saysTheActual: claim.Conjunction is null || resumesTheWhole);
    }

    private void SayOnce(Claim claim, bool saysTheActual)
    {
        // actual is already described text, not source code — never re-parse it. After a
        // conjunction the claim goes on about the same actual, which is not said twice.
        recording.Claim(new(StepLayout.Word) { Body = saysTheActual ? claim.Actual : string.Empty });
        AddWord(claim.Verb.AsWords(), claim.Conjunction == Comma ? ", " : " ");
        AddWord(claim.Expected.Describe());
    }

    // What holds of each item is a line of its own below the items, however short, two steps in, so that a
    // conjunction about the items as a whole can stand one step in.
    private void SayOfEachItem(Claim claim)
    {
        recording.Claim(new(StepLayout.Word) { Body = $"for each of {claim.Actual}:" });
        recording.Add(new(StepLayout.Phrase) { Body = claim.Expected.DescribeOnEachItem(), Indentation = 2 });
    }

    // A comma binds the alternative to the one before it on its line; any other conjunction is a phrase of its own.
    private void Say(string? conjunction, int indentation)
    {
        if (conjunction is null)
            return;

        recording.Add(conjunction == Comma
            ? new(StepLayout.Word) { Introduces = true }
            : new(StepLayout.Phrase) { Body = conjunction, Indentation = indentation, Introduces = true });
    }

    private sealed record Claim(string Actual, string Verb, string? Expected, string? Conjunction, bool OfEachItem);

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

    private void AddWord(string body, string binder = " ")
        => recording.Add(new(StepLayout.Word) { Body = body, Binder = binder });

    private static string DescribeInvocation(string? timesExpr)
        => timesExpr.NormalizeTimes() switch
        {
            "" or "AtLeastOnce" => "was invoked",
            "Never" => "was not invoked",
            "Once" => "was invoked once",
            var normalized => $"was invoked {normalized}",
        };
}
