namespace TSpec.Internal.Specification;

/// <summary>
/// Phase two: turns described clauses into specification text. Everything
/// positional lives here — which lead word a clause gets, whether a mocked
/// service is named again — so that the same clauses rendered in a different
/// arrangement come out correct rather than merely re-ordered. Where one
/// statement ends is not decided here: it arrives as the clause boundary.
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
    {
        var position = new Position();
        List<TextUnit> units = [];
        foreach (var clause in clauses)
        {
            // By position, not by value: a clause can repeat its own wording
            var head = true;
            foreach (var step in Steps(clause, returns))
            {
                Append(units, step, position, isHead: head && step.Layout != StepLayout.Silent);
                head &= step.Layout == StepLayout.Silent;
            }
        }

        if (because is not null)
            AppendWord(units, $"because {because}", ", ");

        return new(units);
    }

    private static IEnumerable<SpecificationStep> Steps(SpecificationClause clause, string? returns)
        => returns is not null && clause.Family == StepFamily.When
            ? [.. clause.Steps, new SpecificationStep(StepLayout.Word)
                {
                    Body = $"returns {returns}",
                    Binder = ", ",
                }]
            : clause.Steps;

    private static void Append(
        List<TextUnit> units, SpecificationStep step, Position position, bool isHead)
    {
        if (step.EndsMockRun)
            position.EndMockRun();
        if (step.Layout == StepLayout.Silent)
            return;

        var content = Content(step, position);
        if (step.Layout == StepLayout.Word && !isHead)
            AppendWord(units, content, step.Binder);
        else
            units.Add(step.Layout switch
            {
                StepLayout.Phrase => TextUnit.Line(content, step.Indentation),
                StepLayout.SentenceOrPhrase =>
                    char.IsUpper(content[0]) ? Sentence(content) : TextUnit.Line(content, 1),
                // A word heading its statement has nothing to append to
                _ => Sentence(content),
            });
    }

    /// <summary>
    /// Punctuation joining a word to what stands before it belongs to that phrase, so it is written
    /// there. It then ends a line rather than starting one, whichever way the text is laid out.
    /// </summary>
    private static void AppendWord(List<TextUnit> units, string content, string binder)
    {
        if (binder.TrimEnd() is { Length: > 0 } punctuation && units.Count > 0)
            units[^1] = units[^1] with { Text = units[^1].Text + punctuation };
        units.Add(TextUnit.Word(content));
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
