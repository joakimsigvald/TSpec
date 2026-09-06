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
        => When(_ => InEnglish(value.FormatValue)).Then().Result.Is(expected);

    [Fact]
    public void ThenReadADateTheSameInEveryCulture()
        => When(_ => InEnglish(new DateTime(2026, 9, 5, 13, 45, 0).FormatValue))
            .Then().Result.Is("2026-09-05 13:45:00");

    [Fact]
    public void ThenReadADayTheSameInEveryCulture()
        => When(_ => InEnglish(new DateOnly(2026, 9, 5).FormatValue))
            .Then().Result.Is("2026-09-05");

    /// <summary>Seconds and all: a time of day states every part it has.</summary>
    [Fact]
    public void ThenReadATimeOfDayTheSameInEveryCulture()
        => When(_ => InEnglish(new TimeOnly(13, 45, 0).FormatValue))
            .Then().Result.Is("13:45:00");

    [Fact]
    public void ThenReadADateWithAnOffsetTheSameInEveryCulture()
        => When(_ => InEnglish(
                new DateTimeOffset(2026, 9, 5, 13, 45, 0, TimeSpan.FromHours(2)).FormatValue))
            .Then().Result.Is("2026-09-05 13:45:00 +02:00");

    /// <summary>
    /// A type that formats its own parts — a record's generated <c>ToString</c> renders each member
    /// with the ambient culture — reaches the document through that method, so the culture has to be
    /// held invariant around it rather than only around the values TSpec formats itself.
    /// </summary>
    [Fact]
    public void ThenReadAValueThatFormatsItsOwnPartsTheSameInEveryCulture()
        => When(_ => InEnglish(new Admission(new DateTime(2026, 9, 5, 13, 45, 0)).FormatValue))
            .Then().Result.Is("Admission { Admitted = 2026-09-05 13:45:00 }");

    private sealed record Admission(DateTime Admitted);

    /// <summary>
    /// A culture that renders dates and numbers differently from the invariant form, so a test that
    /// leaked the ambient culture would say so. The machine this suite grew up on is sv-SE, which
    /// renders dates the same way the invariant form does — running the check there proved nothing.
    /// The thread is its own because the suite runs in parallel and CurrentCulture would leak.
    /// </summary>
    private static string InEnglish(Func<string> format)
    {
        var formatted = string.Empty;
        Thread thread = new(() =>
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            formatted = format();
        });
        thread.Start();
        thread.Join();
        return formatted;
    }
}
