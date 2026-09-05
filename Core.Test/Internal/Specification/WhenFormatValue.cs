using System.Globalization;
using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification;

/// <summary>
/// Values are formatted for a document that has to come out byte-identical on every machine, so a
/// number or a date reads the same whatever the ambient culture is. The culture is set on a thread
/// of its own, since the suite runs in parallel and CurrentCulture would otherwise leak.
/// </summary>
public class WhenFormatValue : Spec<string>
{
    [Theory]
    [InlineData(1.99, "1.99")]
    [InlineData(1.5f, "1.5")]
    public void ThenReadANumberTheSameInEveryCulture(object value, string expected)
        => When(_ => InSwedish(value.FormatValue)).Then().Result.Is(expected);

    [Fact]
    public void ThenReadADateTheSameInEveryCulture()
        => When(_ => InSwedish(new DateTime(2026, 9, 5, 13, 45, 0).FormatValue))
            .Then().Result.Is("2026-09-05 13:45:00");

    private static string InSwedish(Func<string> format)
    {
        var formatted = string.Empty;
        Thread thread = new(() =>
        {
            CultureInfo.CurrentCulture = new CultureInfo("sv-SE");
            formatted = format();
        });
        thread.Start();
        thread.Join();
        return formatted;
    }
}
