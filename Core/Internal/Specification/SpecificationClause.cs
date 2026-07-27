namespace TSpec.Internal.Specification;

/// <summary>
/// One expression of a specification — a <c>Using</c>, a <c>Given</c>, a mock setup with all its
/// returns and throws — as the unit a reader recognizes. A clause is the step that starts a line
/// plus everything that appends to it, so line breaks and a leading "and" are irrelevant to it:
/// those are decisions the renderer makes about a position, not properties of the expression.
/// </summary>
internal sealed class SpecificationClause(IReadOnlyList<SpecificationStep> steps)
{
    internal IReadOnlyList<SpecificationStep> Steps => steps;

    /// The family of the clause's head. Silent steps travel with a clause but never speak for it.
    internal StepFamily Family { get; } =
        steps.FirstOrDefault(step => step.Layout != StepLayout.Silent)?.Family ?? StepFamily.None;

    /// Two clauses are the same expression when they were described identically. Steps are records
    /// over strings and enums, so this is a structural comparison.
    internal bool Matches(SpecificationClause other) => Steps.SequenceEqual(other.Steps);

    /// <summary>
    /// Groups steps into clauses. A silent step attaches to the clause that follows it, because it
    /// exists to affect how that clause renders.
    /// </summary>
    internal static IReadOnlyList<SpecificationClause> Split(IEnumerable<SpecificationStep> steps)
    {
        List<List<SpecificationStep>> clauses = [];
        List<SpecificationStep> pending = [];
        var isAssertionChainOpen = false;
        foreach (var step in steps)
        {
            if (step.Layout == StepLayout.Silent)
            {
                pending.Add(step);
                continue;
            }
            if (StartsLine(step, isAssertionChainOpen) || clauses.Count == 0)
                clauses.Add([]);

            clauses[^1].AddRange(pending);
            clauses[^1].Add(step);
            pending.Clear();
            if (step.Layout == StepLayout.AssertionHead)
                isAssertionChainOpen = false;
            if (step.OpensAssertionChain)
                isAssertionChainOpen = true;
        }
        if (pending.Count > 0 && clauses.Count > 0)
            clauses[^1].AddRange(pending);

        return [.. clauses.Select(clause => new SpecificationClause(clause))];
    }

    /// Mirrors the renderer's own rule, so a clause is exactly what shows up as one statement:
    /// an assertion continues the <c>Then</c> that opened the chain, and starts a line without one.
    private static bool StartsLine(SpecificationStep step, bool isAssertionChainOpen) => step.Layout switch
    {
        StepLayout.Word => false,
        StepLayout.AssertionHead => !isAssertionChainOpen,
        _ => true,
    };
}
