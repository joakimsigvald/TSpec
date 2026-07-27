namespace TSpec.Internal.Specification;

/// <summary>
/// Phrases for the action steps: When/Having/Until and side-effect taps.
/// Each keyword is the name of the pipeline method that produced the step.
/// </summary>
internal class ActionPhrases(SpecificationRecording recording, TextBuilder textBuilder)
{
    internal void AddWhen(string actExpr)
        => recording.Record(() => textBuilder.AddSentence($"when {actExpr.DescribeCall()}"));

    internal void AddHaving(string setUpExpr)
        => recording.Record(() => textBuilder.AddSentence($"having {setUpExpr.DescribeCall()}"));

    internal void AddUntil(string tearDownExpr)
        => recording.Record(() => textBuilder.AddSentence($"until {tearDownExpr.DescribeCall()}"));

    internal void AddTap(string expr)
        => recording.Record(() => textBuilder.AddWord($"tap({expr})"));
}
