namespace TSpec.Internal.Specification;

/// <summary>
/// Phase two: turns described steps into specification text. Everything
/// positional lives here — which lead word a step gets, whether a mocked
/// service is named again, whether an assertion starts a sentence or continues
/// one — so that the same steps rendered in a different arrangement come out
/// correct rather than merely re-ordered.
/// </summary>
/// <remarks>
/// It composes, it does not lay out. What comes back says everything it will say and breaks no
/// lines, so a caller that still has text to remove or a width of its own can do that before
/// <see cref="ComposedText.Render"/> measures anything.
/// </remarks>
internal static class SpecificationRenderer
{
    internal static ComposedText Compose(
        IReadOnlyList<SpecificationClause> clauses, string? because, string? returns = null)
        => Compose(Steps(clauses, returns), because);

    internal static ComposedText Compose(IEnumerable<SpecificationStep> steps, string? because)
    {
        var position = new Position();
        List<TextUnit> units = [];
        foreach (var step in steps)
            Append(units, step, position);

        if (because is not null)
            units.Add(TextUnit.Word($"because {because}", ", "));

        return new(units);
    }

    private static IEnumerable<SpecificationStep> Steps(
        IReadOnlyList<SpecificationClause> clauses, string? returns)
        => clauses.SelectMany(clause => returns is not null && clause.Family == StepFamily.When
            ? [.. clause.Steps, new SpecificationStep(StepLayout.Word)
                {
                    Body = $"returns {returns}",
                    Binder = ", ",
                }]
            : clause.Steps);

    private static void Append(List<TextUnit> units, SpecificationStep step, Position position)
    {
        if (step.EndsMockRun)
            position.EndMockRun();
        if (step.Layout == StepLayout.Silent)
            return;

        var content = Content(step, position);
        switch (step.Layout)
        {
            case StepLayout.Sentence:
                units.Add(Sentence(content));
                break;
            case StepLayout.Phrase:
                units.Add(TextUnit.Line(content, step.Indentation));
                break;
            case StepLayout.SentenceOrPhrase:
                units.Add(char.IsUpper(content[0]) ? Sentence(content) : TextUnit.Line(content, 1));
                break;
            case StepLayout.AssertionHead:
                units.Add(position.IsAssertionChainOpen
                    ? TextUnit.Word(content, " ")
                    : Sentence(content));
                position.CloseAssertionChain();
                break;
            default:
                units.Add(TextUnit.Word(content, step.Binder));
                break;
        }
        if (step.OpensAssertionChain)
            position.OpenAssertionChain();
    }

    /// A sentence is capitalized while it is composed, not while it is laid out: case is a fact
    /// about the words, and layout has to be given the very text it will measure.
    private static TextUnit Sentence(string content) => TextUnit.Line(content.Capitalize(), 0);

    private static string Content(SpecificationStep step, Position position)
    {
        var tail = position.MockName(step.MockService, step.MockBinder) + step.Body;
        var lead = position.LeadWord(step.Family);
        if (lead is null)
            return tail;

        return tail.Length == 0 ? lead : $"{lead} {tail}";
    }

    /// The rendering state that depends on what has already been rendered.
    private sealed class Position
    {
        private readonly HashSet<StepFamily> _started = [];
        private string? _currentMock;

        internal bool IsAssertionChainOpen { get; private set; }
        internal void OpenAssertionChain() => IsAssertionChainOpen = true;
        internal void CloseAssertionChain() => IsAssertionChainOpen = false;

        internal void EndMockRun() => _currentMock = null;

        /// A family says its word once and is joined by its binder from then on, so consecutive steps
        /// of one kind become a single statement. They are listed in the order they were declared,
        /// and the binder is what says how that stands to the order they run in.
        internal string? LeadWord(StepFamily family)
            => family == StepFamily.None ? null
            : _started.Add(family) ? family.Keyword() : family.Binder();

        /// A service is named the first time it is spoken about, and again after
        /// any non-mock setup step has interrupted the run.
        internal string MockName(string? service, char binder)
        {
            if (service is null)
                return string.Empty;

            var name = service == _currentMock ? string.Empty : $"{service}{binder}";
            _currentMock = service;
            return name;
        }
    }
}
