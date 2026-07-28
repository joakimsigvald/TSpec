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
    // A branch path is dotted, and each segment is a sentence of its own
    [InlineData("GivenCartExists.WithItems", "Given cart exists. With items")]
    public void ThenReadAsAHeading(string identifier, string expected)
        => When(_ => identifier.AsHeading()).Then().Result.Is(expected);
}
