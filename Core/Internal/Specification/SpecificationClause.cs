namespace TSpec.Internal.Specification;

/// <summary>
/// One expression of a specification — a <c>Using</c>, a <c>Given</c>, a mock setup with all its
/// returns and throws, one assertion — as the unit a reader recognizes, and as the statement that
/// gets a line of its own. A clause is the step that heads it plus everything that appends to it,
/// so line breaks and a leading "and" are irrelevant to it: those are decisions the renderer makes
/// about a position, not properties of the expression.
/// </summary>
/// <remarks>
/// Cut where the steps are recorded, so where a claim ends is carried rather than re-derived.
/// </remarks>
internal sealed class SpecificationClause(IReadOnlyList<SpecificationStep> steps)
{
    internal IReadOnlyList<SpecificationStep> Steps => steps;

    /// Taken from the clause's head. Silent steps travel with a clause but never speak for it.
    internal StepFamily Family { get; } =
        steps.FirstOrDefault(step => step.Layout != StepLayout.Silent)?.Family ?? StepFamily.None;

    /// What the clause is for, which its lead word already tells us.
    internal StepPhase Phase => Family switch
    {
        StepFamily.Using or StepFamily.Given => StepPhase.Arrange,
        StepFamily.When or StepFamily.Having or StepFamily.Until => StepPhase.Act,
        _ => StepPhase.Assert,
    };

    /// The step the clause is built around. Silent steps travel with it but never speak for it.
    internal SpecificationStep Head { get; } = HeadOf(steps);

    /// <summary>
    /// The series of clauses this one belongs to, where their order decides what happens: the setups
    /// of one member of a mock, since the latest matching a call answers it, or the Having steps,
    /// which run one after another. Null where the clause's place decides nothing.
    /// </summary>
    internal string? Series { get; } = SeriesOf(HeadOf(steps));

    /// Two clauses are the same expression when they were described identically. Steps are records
    /// over strings and enums, so this is a structural comparison.
    internal bool Matches(SpecificationClause other) => Steps.SequenceEqual(other.Steps);

    private static SpecificationStep HeadOf(IReadOnlyList<SpecificationStep> steps)
        => steps.First(step => step.Layout != StepLayout.Silent);

    private static string? SeriesOf(SpecificationStep head)
        => head.Family == StepFamily.Having ? head.Family.Keyword()
            : head.MockService is { } mock ? $"{mock}.{SetUpMemberOf(head)}"
            : null;

    /// <summary>
    /// The member a mock setup is for, whatever its arguments: what comes before them, or before the
    /// step a chain takes from it. A default for every call on the mock is a member of its own.
    /// </summary>
    private static string SetUpMemberOf(SpecificationStep head)
        => head.IsMockDefault ? string.Empty : head.Body.Split(['(', '[', '.'])[0];
}
