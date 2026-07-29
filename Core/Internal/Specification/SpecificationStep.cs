namespace TSpec.Internal.Specification;

/// <summary>
/// One recorded pipeline step, described but not yet laid out. This is the
/// hand-off between the two phases: the phrase classes decide what a step
/// *says*, and <see cref="SpecificationRenderer"/> decides how it reads in a
/// given position — lead word, mock-name elision, line breaks, indentation.
/// </summary>
/// <remarks>
/// Nothing here knows its position, which is what lets the same steps be
/// rendered in a different arrangement than the one they were recorded in.
/// </remarks>
internal sealed record SpecificationStep(StepLayout Layout)
{
    /// The step's text, without its lead word and without the mocked service name.
    internal string Body { get; init; } = string.Empty;

    internal StepFamily Family { get; init; } = StepFamily.None;

    /// Indentation in levels, for <see cref="StepLayout.Phrase"/>.
    internal int Indentation { get; init; } = 1;

    /// Separator placed before the text, for <see cref="StepLayout.Word"/>.
    internal string Binder { get; init; } = " ";

    /// The mocked service this step speaks about, and the character joining it to
    /// the body. Consecutive steps about the same service drop the repeated name.
    internal string? MockService { get; init; }
    internal char MockBinder { get; init; } = ' ';

    /// A setup step that is not about a mock ends the run, so a later mock step
    /// names its service again.
    internal bool EndsMockRun { get; init; }

    /// Then/that/conjunction open a chain, so the assertion that follows appends
    /// to the line instead of starting a new sentence.
    internal bool OpensAssertionChain { get; init; }
}

/// How a step joins the text around it.
internal enum StepLayout
{
    /// Contributes no text; recorded only for its effect on rendering state.
    Silent,
    /// Starts a line, capitalized.
    Sentence,
    /// Starts an indented line.
    Phrase,
    /// Sentence when the composed text starts upper-case, phrase otherwise —
    /// which is how a lead word of "Given" and one of "and" part ways.
    SentenceOrPhrase,
    /// Appends to the line in progress.
    Word,
    /// Appends while an assertion chain is open, starts a sentence otherwise.
    AssertionHead,
}

/// <summary>
/// The lead word a step introduces. Which word that is depends on position —
/// the first step of a family gets the family's word and the rest get "and" —
/// so the choice belongs to the renderer, not to the recording.
/// <see cref="None"/> is the assertion that heads its own line, having nothing
/// in front of it: <c>Result has all …</c> used without a <c>Then</c>.
/// </summary>
internal enum StepFamily { None, Using, Given, When, Having, Until, Then }

/// The words a statement can open with, shared by the renderer that writes them and the document
/// that drops one where it would be said twice.
internal static class StepFamilies
{
    internal static string Keyword(this StepFamily family) => family switch
    {
        StepFamily.Using => "Using",
        StepFamily.Given => "Given",
        StepFamily.When => "When",
        StepFamily.Having => "Having",
        StepFamily.Until => "Until",
        _ => "Then",
    };

    /// Every lead word there is — which is also every word a heading built from a test name can open
    /// with, since <c>ThenRespondOk</c> reads "Then respond ok".
    internal static IReadOnlyList<string> Keywords { get; } =
        [.. Enum.GetValues<StepFamily>().Where(family => family != StepFamily.None).Select(Keyword)];
}

/// <summary>
/// Which part of the test a clause belongs to, derived from its family. It is a
/// property of a clause rather than of a step: a continuation such as
/// <c>returns 1</c> belongs to whatever phase its head does.
/// </summary>
internal enum StepPhase { Arrange, Act, Assert }
