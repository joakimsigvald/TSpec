namespace TSpec.Internal.Specification;

/// <summary>
/// Phase two: turns described steps into specification text. Everything
/// positional lives here — which lead word a step gets, whether a mocked
/// service is named again, whether an assertion starts a sentence or continues
/// one — so that the same steps rendered in a different arrangement come out
/// correct rather than merely re-ordered.
/// </summary>
internal static class SpecificationRenderer
{
    internal static string Render(
        IEnumerable<SpecificationStep> steps, string? because, TextBuilder text)
    {
        var position = new Position();
        foreach (var step in steps)
            Append(text, step, position);

        if (because is not null)
            text.AddWord($"because {because}", ", ");

        return text.ToString();
    }

    private static void Append(TextBuilder text, SpecificationStep step, Position position)
    {
        if (step.EndsMockRun)
            position.EndMockRun();
        if (step.Layout == StepLayout.Silent)
            return;

        var content = Compose(step, position);
        switch (step.Layout)
        {
            case StepLayout.Sentence:
                text.AddSentence(content);
                break;
            case StepLayout.Phrase:
                text.AddPhrase(content, step.Indentation);
                break;
            case StepLayout.SentenceOrPhrase:
                text.AddPhraseOrSentence(content);
                break;
            case StepLayout.AssertionHead:
                if (position.IsAssertionChainOpen)
                    text.AddWord(content);
                else
                    text.AddSentence(content);
                position.CloseAssertionChain();
                break;
            default:
                text.AddWord(content, step.Binder);
                break;
        }
        if (step.OpensAssertionChain)
            position.OpenAssertionChain();
    }

    private static string Compose(SpecificationStep step, Position position)
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
