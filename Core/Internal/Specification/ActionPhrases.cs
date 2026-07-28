namespace TSpec.Internal.Specification;

/// <summary>
/// Describes the action steps: When/Having/Until and side-effect taps.
/// Each keyword is the name of the pipeline method that produced the step.
/// </summary>
internal class ActionPhrases(SpecificationRecording recording)
{
    internal void AddWhen(string actExpr) => Add(StepFamily.When, actExpr);

    internal void AddHaving(string setUpExpr) => Add(StepFamily.Having, setUpExpr);

    internal void AddUntil(string tearDownExpr) => Add(StepFamily.Until, tearDownExpr);

    internal void AddTap(string expr)
        => recording.Record(() => recording.Add(new(StepLayout.Word) { Body = $"tap({expr})" }));

    private void Add(StepFamily family, string expr)
        => recording.Record(() => recording.Add(new(StepLayout.SentenceOrPhrase)
        {
            Family = family,
            Body = expr.DescribeCall() ?? string.Empty,
        }));
}
