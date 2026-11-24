using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification.TextBuilder;

public class WhenAddText
{
    private static string AddText(string? text, string? existingText = null)
    {
        var builder = new TSpec.Internal.Specification.TextBuilder(10, 1);
        if (existingText is not null)
            builder.AddText(existingText);
        return builder.AddText(text).ToString().NormalizeLineEndings();
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("123456789", "123456789")]
    [InlineData("1234567890", "1234567890")]
    [InlineData("12345678901",
        """
        1234567890
           1
        """)]
    [InlineData("123456789012345678901",
        """
        1234567890
           1234567
           8901
        """)]
    [InlineData("12345 678901",
        """
        12345
           678901
        """)]
    [InlineData("123 567 901",
        """
        123 567
           901
        """)]
    [InlineData("12345.78901",
        """
        12345.7890
           1
        """)]
    [InlineData("Abc[..7890]",
        """
        Abc[
           ..7890]
        """)]
    [InlineData("12345.A8901",
        """
        12345.
           A8901
        """)]
    [InlineData("ABC(567890)",
        """
        ABC(
           567890)
        """)]
    [InlineData("ABC[567890]",
        """
        ABC[
           567890]
        """)]
    [InlineData("ABC{567890}",
        """
        ABC{
           567890}
        """)]
    [InlineData("ABC<5678901>",
        """
        ABC<567890
           1>
        """)]
    public void ThenReturnDescription(string? text, string expected)
        => Xunit.Assert.Equal(expected.NormalizeLineEndings(), AddText(text));

    [Theory]
    [InlineData(null, null, "")]
    [InlineData("12345678", "901",
        """
        12345678
           901
        """)]
    public void GivenHasTextAndNextWordDoesNotFit_ThenBreakBeforeWord(
        string? existingText, string? newText, string expected)
        => Xunit.Assert.Equal(expected.NormalizeLineEndings(), AddText(newText, existingText));

    /// « opens a group, » closes it, ¦ marks a break point of the group it stands in, ¶ one that
    /// lines up with the line the group opened on — stand-ins for the Wrap markers, which are
    /// unprintable.
    private static string Visible(string text)
        => text
            .Replace('«', Wrap.Enter)
            .Replace('»', Wrap.Exit)
            .Replace('¦', Wrap.Point)
            .Replace('¶', Wrap.Align);

    private static string AddMarkedText(string text) => AddText(Visible(text));

    [Theory]
    [InlineData("A(«¦BC»)", "A(BC)")]
    [InlineData("ABC(«¦DE, ¦FG»)",
        """
        ABC(
           DE,
           FG)
        """)]
    [InlineData("A(«¦BB, ¦C(«¦D»)»)",
        """
        A(
           BB,
           C(D))
        """)]
    [InlineData("A(«¦BB, ¦CC(«¦DD, ¦EE»)»)",
        """
        A(
           BB,
           CC(
            DD,
            EE))
        """)]
    [InlineData("ABCDE« ¦{«¦ F ¶»}»",
        """
        ABCDE
           { F }
        """)]
    [InlineData("AB« ¦{«¦ CDE, ¦FGH ¶»}»",
        """
        AB
           {
            CDE,
            FGH
           }
        """)]
    [InlineData("ABCD(«¦«EF, ¶GH»»)",
        """
        ABCD(
           EF, GH)
        """)]
    [InlineData("A(«¦«BCD, ¶EFG»»)",
        """
        A(
           BCD,
           EFG)
        """)]
    public void GivenBreakPoints_ThenBreakAGroupThatDoesNotFitAtEachOfItsOwn(string text, string expected)
        => Xunit.Assert.Equal(expected.NormalizeLineEndings(), AddMarkedText(text));

    [Fact]
    public void GivenAPartBroke_ThenStartTheNextPartOnALineOfItsOwn()
    {
        var builder = new TSpec.Internal.Specification.TextBuilder(10, 1);
        builder.Add(TextUnit.Line(Visible("ABC(«¦DE, ¦FG»)"), 0));
        builder.Add(TextUnit.Word("HI"));
        Xunit.Assert.Equal(
            "ABC(\n   DE,\n   FG)\n   HI",
            builder.Build(opensSentence: false).NormalizeLineEndings());
    }

    [Theory]
    [InlineData("ABCDE FG(H)",
        """
        ABCDE
           FG(H)
        """)]
    public void GivenNoBreakPoints_ThenWhitespaceOutranksPunctuation(string text, string expected)
        => Xunit.Assert.Equal(expected.NormalizeLineEndings(), AddText(text));

    [Fact]
    public void GivenNarrowerWrapIndentation_ThenContinuationLinesIndentLess()
        => Xunit.Assert.Equal(
            "12345\n  678901",
            new TSpec.Internal.Specification.TextBuilder(10, 1, wrapIndentation: 2)
                .AddText("12345 678901").ToString().NormalizeLineEndings());

    /// Text without break points continues relative to the line it continues — its step plus the
    /// wrap delta — and does not compound: every continuation takes the same column.
    [Fact]
    public void GivenIndentedLine_ThenContinuationsIndentRelativeToIt()
    {
        var builder = new TSpec.Internal.Specification.TextBuilder(10, 1, wrapIndentation: 2);
        builder.Add(TextUnit.Line("AB CD EFG HI JKL MN", 1));
        Xunit.Assert.Equal(
            "AB CD\n   EFG HI\n   JKL MN",
            builder.Build(opensSentence: false).NormalizeLineEndings());
    }

    /// Laid out ten columns wide, tolerating three more — so a statement breaks at ten,
    /// but only once it is past thirteen.
    private static string Tolerated(params TextUnit[] units)
    {
        var builder = new TSpec.Internal.Specification.TextBuilder(
            10, 1, wrapIndentation: 2, tolerance: 3);
        foreach (var unit in units)
            builder.Add(unit);
        return builder.Build(opensSentence: false).NormalizeLineEndings();
    }

    [Theory]
    [InlineData("12345 678901", "12345 678901")]
    [InlineData("12345 6789012", "12345 6789012")]
    [InlineData("12345 67890123",
        """
        12345
          67890123
        """)]
    public void GivenTolerance_ThenLeaveAStatementWithinItWhole(string text, string expected)
        => Xunit.Assert.Equal(expected.NormalizeLineEndings(), Tolerated(TextUnit.Line(text, 0)));

    /// A statement arrives a piece at a time, and the pieces share the one tolerance the line has —
    /// the whole of it is one expression, so what fits is not broken between two of them.
    [Fact]
    public void GivenTolerance_ThenLetTheWholeStatementRunIntoIt()
        => Xunit.Assert.Equal(
            "12345 678901", Tolerated(TextUnit.Line("12345", 0), TextUnit.Word("678901")));

    /// The tolerance decides whether a line breaks, never where: past it, the break falls at the
    /// width, so what continues is no wider than an untolerated line.
    [Fact]
    public void GivenAStatementPastTheTolerance_ThenWrapItAtTheWidth()
        => Xunit.Assert.Equal(
            "12345\n  67890\n  12345".NormalizeLineEndings(),
            Tolerated(TextUnit.Line("12345 67890 12345", 0)));

    /// A statement that wraps spends its own tolerance and no one else's: the next one starts a
    /// line of its own, and a line that has not broken has the whole tolerance to run into.
    [Fact]
    public void GivenOneStatementIsPastTheTolerance_ThenStillLeaveTheNextOneWhole()
        => Xunit.Assert.Equal(
            "12345\n  67890\n  12345\n12345 678901".NormalizeLineEndings(),
            Tolerated(TextUnit.Line("12345 67890 12345", 0), TextUnit.Line("12345 678901", 0)));
}