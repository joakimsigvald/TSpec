using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

internal static class DocumentText
{
    internal const int DocumentWidth = 90;
    internal const int ItemIndentation = 2;
    internal const int FenceWidth = DocumentWidth - ItemIndentation;
    internal const int ClaimWidth = FenceWidth - 2;

    internal static ComposedText Compose(
        IReadOnlyList<SpecificationClause> clauses, string? because, string? returns = null)
        => SpecificationRenderer.Compose(Steps(clauses, returns), because);

    private static IEnumerable<SpecificationStep> Steps(
        IReadOnlyList<SpecificationClause> clauses, string? returns)
        => clauses.SelectMany(clause => returns is not null && clause.Family == StepFamily.When
            ? [.. clause.Steps, new SpecificationStep(StepLayout.Word)
                {
                    Body = $"returns {returns}",
                    Binder = ", ",
                }]
            : clause.Steps);
}
