namespace TSpec.Internal.Specification;

/// <summary>
/// Describes the action steps: When/Having/Until and side-effect taps.
/// Each keyword is the name of the pipeline method that produced the step.
/// </summary>
internal class ActionPhrases(SpecificationRecording recording)
{
    internal void AddWhen(string actExpr) => AddSentence("when", actExpr);

    internal void AddHaving(string setUpExpr) => AddSentence("having", setUpExpr);

    internal void AddUntil(string tearDownExpr) => AddSentence("until", tearDownExpr);

    internal void AddTap(string expr)
        => recording.Record(() => recording.Add(new(StepLayout.Word) { Body = $"tap({expr})" }));

    private void AddSentence(string keyword, string expr)
        => recording.Record(() => recording.Add(
            new(StepLayout.Sentence) { Body = $"{keyword} {expr.DescribeCall()}" }));
}
