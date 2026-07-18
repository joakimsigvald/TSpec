namespace TSpec.Internal.Specification;

/// <summary>
/// Phrases for the action steps: When/After/Before and side-effect taps.
/// </summary>
internal class ActionPhrases(SpecificationRecording recording, TextBuilder textBuilder)
{
    internal void AddWhen(string actExpr)
        => recording.Record(() => textBuilder.AddSentence($"when {actExpr.ParseCall()}"));

    internal void AddAfter(string setUpExpr)
        => recording.Record(() => textBuilder.AddSentence($"after {setUpExpr.ParseCall()}"));

    internal void AddBefore(string tearDownExpr)
        => recording.Record(() => textBuilder.AddSentence($"before {tearDownExpr.ParseCall()}"));

    internal void AddTap(string expr)
        => recording.Record(() => textBuilder.AddWord($"tap({expr})"));
}
