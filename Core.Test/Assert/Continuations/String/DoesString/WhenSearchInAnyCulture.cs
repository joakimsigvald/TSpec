using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.DoesString;

/// <summary>
/// Searching a string is about the characters in it. xunit compares with the current culture unless
/// told otherwise, and a collation that ignores punctuation — Thai's does — then finds a run of
/// hashes inside text holding none, so an assertion passes or fails by where the developer sits.
/// </summary>
public class WhenSearchInAnyCulture : StringSpec
{
    private const string Text = "# My Hotel #", Absent = "###";

    [Fact] public void ThenDoNotContainWhatIsAbsent() => InThai(() => Text.Does().not.Contain(Absent));

    /// <summary>
    /// A zero-width joiner is ignorable to a collator, so a culture-sensitive search finds it at the
    /// start or the end of text that does not hold the character at all.
    /// </summary>
    private const string Ignorable = "‍";

    [Fact] public void ThenDoNotStartWithWhatIsAbsent() => InThai(() => Text.Does().not.StartWith(Ignorable));

    [Fact] public void ThenDoNotEndWithWhatIsAbsent() => InThai(() => Text.Does().not.EndWith(Ignorable));

    /// <summary>On a thread of its own, since the suite runs in parallel and the culture would leak.</summary>
    private static void InThai(Action assert)
    {
        Exception? thrown = null;
        Thread thread = new(() =>
        {
            System.Globalization.CultureInfo.CurrentCulture = new("th-TH");
            try
            {
                assert();
            }
            catch (Exception exception)
            {
                thrown = exception;
            }
        });
        thread.Start();
        thread.Join();
        if (thrown is not null)
            throw thrown;
    }
}
