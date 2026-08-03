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

    /// « enters a nesting level, » exits it, ¦ marks a break point ranked by its depth —
    /// stand-ins for the Wrap markers, which are unprintable.
    private static string AddMarkedText(string text)
        => AddText(text
            .Replace('«', Wrap.Enter)
            .Replace('»', Wrap.Exit)
            .Replace('¦', Wrap.Point));

    [Theory]
    [InlineData("A(«¦BC»)", "A(BC)")]
    [InlineData("AB CD(«¦EF GH»)",
        """
        AB CD(
           EF GH)
        """)]
    [InlineData("ABC(«¦DE, ¦FG»)",
        """
        ABC(DE,
           FG)
        """)]
    [InlineData("A(«¦BB, ¦CC(«¦DD, ¦EE»)»)",
        """
        A(BB,
           CC(DD,
           EE))
        """)]
    [InlineData("AB with« ¦{CD}»",
        """
        AB with
           {CD}
        """)]
    public void GivenBreakPoints_ThenBreakAtLastPointOfShallowestRank(string text, string expected)
        => Xunit.Assert.Equal(expected.NormalizeLineEndings(), AddMarkedText(text));

    [Theory]
    [InlineData("ABCDE FG(H)",
        """
        ABCDE
           FG(H)
        """)]
    public void GivenNoBreakPoints_ThenWhitespaceOutranksPunctuation(string text, string expected)
        => Xunit.Assert.Equal(expected.NormalizeLineEndings(), AddText(text));
}