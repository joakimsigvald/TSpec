using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification;

/// <summary>
/// A collection mention names a type and a count, so the type has to read as a plural: "two Rooms",
/// not "two Room". English spells noun plurals and third-person verbs by one and the same rule, so
/// this shares its orthography with <c>PresentSingularS</c> rather than restating it.
/// </summary>
/// <remarks>
/// Regular nouns only. An irregular one (Child, Person) comes out as though it were regular, and the
/// author cannot fix that by renaming their domain type — but it is the same kind of wrong as the
/// bare singular it replaces, and far rarer.
/// </remarks>
public class WhenPluralizeANoun : Spec<string>
{
    [Theory]
    [InlineData("Room", "Rooms")]
    [InlineData("MyModel", "MyModels")]
    [InlineData("int", "ints")]
    // A sibilant needs a syllable of its own
    [InlineData("Class", "Classes")]
    [InlineData("Box", "Boxes")]
    [InlineData("Match", "Matches")]
    [InlineData("Dish", "Dishes")]
    [InlineData("Quiz", "Quizes")]
    // -y is a plural in -ies only after a consonant
    [InlineData("Query", "Queries")]
    [InlineData("Entity", "Entities")]
    [InlineData("Key", "Keys")]
    [InlineData("Day", "Days")]
    public void ThenSpellItRegularly(string noun, string expected)
        => When(_ => noun.Pluralize()).Then().Result.Is(expected);

    /// <summary>
    /// Anything that is not a plain identifier was never a noun to inflect — a generic or an array
    /// has no spelling that reads as a plural, so it is left exactly as written.
    /// </summary>
    [Theory]
    [InlineData("KeyValuePair<int, string>")]
    [InlineData("Room[]")]
    [InlineData("")]
    public void GivenItIsNotAPlainName_ThenLeaveItAlone(string type)
        => When(_ => type.Pluralize()).Then().Result.Is(type);
}
