using System.Text.RegularExpressions;
using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.DoesString;

public class WhenMatch : StringSpec
{
    [Theory]
    [InlineData("abc123", @"[a-c]+\d+")]
    [InlineData("2026-07-20", @"^\d{4}-\d{2}-\d{2}$")]
    public void GivenMatchingPattern_ThenDoesNotThrow(string text, string pattern)
    {
        text.Does().Match(pattern);
        Specification.Is("Text matches pattern");
    }

    [Theory]
    [InlineData(null, @"\d+")]
    [InlineData("abc", @"\d+")]
    public void GivenNotMatchingPattern_ThenGetException(string? text, string pattern)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => text.Does().Match(pattern));
        ex.HasMessage($"Expected text to match {Describe(pattern)} but found {Describe(text)}", "Text matches pattern");
    }

    [Fact]
    public void GivenNotMatch_ThenDoesNotThrow()
        => "abc".Does().not.Match(@"\d+");

    [Fact]
    public void GivenRegexWithOptions_ThenDoesNotThrow()
    {
        var regex = new Regex("^ABC$", RegexOptions.IgnoreCase);
        "abc".Does().Match(regex);
    }
}
