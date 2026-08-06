using TSpec.Internal.Specification;

namespace TSpec.Internal.Document;

/// <summary>
/// Where composed text becomes characters on a page of a given width. Nothing here knows what a
/// requirement, a heading or a section is — it takes text and a width and gives back a string.
/// </summary>
/// <remarks>
/// Line endings are normalized to LF as text is rendered, since a Windows run would otherwise
/// commit a file differing from a Linux one on every line. That, and full sorting upstream, is
/// what makes the document byte-identical on every machine and so reviewable as a diff.
/// <para>
/// Width lives here and only here. It is the renderer's variable, which is why the representation
/// may hold composed text but never text that has been through <see cref="Fit"/>.
/// </para>
/// </remarks>
internal static class DocumentText
{
    internal const int DocumentWidth = 90;

    private const int Tolerance = 10;

    private const int ItemIndentation = 2;

    private const int ContinuationIndentation = 2;

    internal const int FenceWidth = DocumentWidth - ItemIndentation;

    internal const int ClaimWidth = FenceWidth - 2;

    internal static ComposedText Compose(
        IReadOnlyList<SpecificationClause> clauses, string? because, string? returns = null)
        => SpecificationRenderer.Compose(Steps(clauses, returns), because);

    /// Composed text laid out to a width, which is the one thing the representation may not do.
    internal static string Fit(ComposedText text, int maxLineLength)
        => text.Render(maxLineLength, ContinuationIndentation, Tolerance).NormalizeLineEndings();

    private static IEnumerable<SpecificationStep> Steps(
        IReadOnlyList<SpecificationClause> clauses, string? returns)
        => clauses.SelectMany(clause => returns is not null && clause.Family == StepFamily.When
            ? [.. clause.Steps, new SpecificationStep(StepLayout.Word)
                {
                    Body = $"returns {returns}",
                    Binder = ", ",
                }]
            : clause.Steps);

    /// Fenced where it runs to more than a line, inline where it does not.
    internal static string Block(string specification)
        => specification.Contains('\n')
            ? $"```\n{specification}\n```\n"
            : $"`{specification}`\n";

    internal static string Indent(string block)
        => string.Join("\n",
            block.Split('\n').Select(line => $"{new string(' ', ItemIndentation)}{line}"));

    internal static int Lines(string text) => text.Count(character => character == '\n');

    /// The lead word a heading opens with, if it opens with one.
    internal static string? StatedWord(string heading)
        => StepFamilies.Keywords.FirstOrDefault(
            word => heading.StartsWith($"{word} ", StringComparison.Ordinal));

    /// A heading's lead word is not said again below it.
    internal static string Without(string? stated, string specification)
        => stated is not null && specification.StartsWith($"{stated} ", StringComparison.Ordinal)
            ? specification[(stated.Length + 1)..]
            : specification;
}
