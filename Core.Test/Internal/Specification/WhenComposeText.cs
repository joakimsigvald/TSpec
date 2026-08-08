using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification;

/// <summary>
/// Punctuation joining a word to what stands before it is written into that phrase while the text is
/// composed, so layout never sees it as anything but text. A line can then end with it but never
/// open with it, and keeping it costs no column.
/// </summary>
public class WhenComposeText
{
    private static string Compose(int width, params SpecificationStep[] steps)
        => SpecificationRenderer
            .Compose([new SpecificationClause(steps)], because: null)
            .Render(width, wrapIndentation: 1)
            .NormalizeLineEndings();

    private static SpecificationStep Statement(string body)
        => new(StepLayout.Sentence) { Body = body };

    private static SpecificationStep Joined(string body)
        => new(StepLayout.Word) { Body = body, Binder = ", " };

    /// The joiner does not fit, so the line breaks before the phrase it joins — never past the width.
    [Fact]
    public void GivenTheJoinerWouldNotFit_ThenBreakBeforeWhatItJoins()
        => Xunit.Assert.Equal(
            """
            12345
              789012, ab
            """.NormalizeLineEndings(),
            Compose(12, Statement("12345 789012"), Joined("ab")));

    /// Where it fits, only the word it introduces travels to the next line.
    [Fact]
    public void GivenTheJoinedWordDoesNotFit_ThenLeaveThePunctuationBehind()
        => Xunit.Assert.Equal(
            """
            1234567890,
              abcdef
            """.NormalizeLineEndings(),
            Compose(12, Statement("1234567890"), Joined("abcdef")));
}
