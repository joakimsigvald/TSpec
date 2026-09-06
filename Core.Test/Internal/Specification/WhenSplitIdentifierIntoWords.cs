using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification;

/// <summary>
/// Identifiers become prose in two places that matter to a reader: the headings of a specification
/// document, and the verbs of a rendered assertion. Both go through the same splitter.
/// </summary>
public class WhenSplitIdentifierIntoWords : Spec<string>
{
    [Theory]
    [InlineData("WhenAddRoom", "when add room")]
    [InlineData("GivenNoSuchRoom", "given no such room")]
    [InlineData("TryGet", "try get")]
    [InlineData("Room", "room")]
    // A lone capital is a word of its own
    [InlineData("GivenASnake", "given a snake")]
    [InlineData("ThenReturnA", "then return a")]
    // Successive capitals are one acronym, and keep the case that says so
    [InlineData("ThenReturnHTTPStatus", "then return HTTP status")]
    [InlineData("GivenHTTP", "given HTTP")]
    [InlineData("ThenRespondOK", "then respond OK")]
    [InlineData("ThenRespondOk", "then respond ok")]
    // An underscore separates clauses, not words
    [InlineData("GivenASnake_WithWings", "given a snake, with wings")]
    [InlineData("GivenA_GivenB_GivenC", "given a, given b, given c")]
    [InlineData("GivenNumber123", "given number 123")]
    [InlineData("_leading", "leading")]
    public void ThenReadAsWords(string identifier, string expected)
        => When(_ => identifier.AsWords()).Then().Result.Is(expected);

    [Theory]
    [InlineData("MyHotel", "My Hotel")]
    [InlineData("MyHTTPServer", "My HTTP Server")]
    public void ThenReadAsATitle(string identifier, string expected)
        => When(_ => identifier.AsTitle()).Then().Result.Is(expected);

    [Theory]
    [InlineData("GivenNoSuchRoom", "Given no such room")]
    [InlineData("GivenASnake_WithWings", "Given a snake, with wings")]
    // A branch path is dotted, and reads as one sentence: a nested given refines the one above it
    // rather than stating something new, which is what an underscore says within one name too
    [InlineData("GivenCartExists.WithItems", "Given cart exists, with items")]
    [InlineData("GivenTheRoomExists.ButIsAlreadyBooked", "Given the room exists, but is already booked")]
    [InlineData("GivenA.WithB.ButC", "Given a, with b, but c")]
    public void ThenReadAsAHeading(string identifier, string expected)
        => When(_ => identifier.AsHeading()).Then().Result.Is(expected);

    /// <summary>
    /// Turkish maps I to a dotless ı and i to a dotted İ, so an identifier carrying either letter
    /// comes out differently there unless the casing states the culture it means. A document that
    /// read one way in Istanbul and another in Stockholm would not be the byte-identical artefact
    /// the freshness check compares.
    /// </summary>
    [Theory]
    [InlineData("ThenListItems", "then list items")]
    [InlineData("GivenTheRoomIsBooked", "given the room is booked")]
    public void ThenReadAsWordsTheSameInEveryCulture(string identifier, string expected)
        => When(_ => InTurkish(() => identifier.AsWords())).Then().Result.Is(expected);

    [Fact]
    public void ThenReadAsAHeadingTheSameInEveryCulture()
        => When(_ => InTurkish("GivenItIsIdle".AsHeading)).Then().Result.Is("Given it is idle");

    private static string InTurkish(Func<string> render)
    {
        var rendered = string.Empty;
        Thread thread = new(() =>
        {
            System.Globalization.CultureInfo.CurrentCulture = new("tr-TR");
            rendered = render();
        });
        thread.Start();
        thread.Join();
        return rendered;
    }
}
