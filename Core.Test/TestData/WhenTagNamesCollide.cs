using TSpec.Assert;

namespace TSpec.Test.TestData;

/// <summary>
/// A tag's name identifies its value in a failure report, so two tags cannot share one within a
/// test. The collision that matters is the one nobody writes on purpose: tags declared inside a
/// method all take that method's name, because a variable name is only visible to the compiler in
/// a field declaration.
/// </summary>
public class WhenTagNamesCollide : Spec<int>
{
    [Fact]
    public void GivenTwoTagsDeclaredInAMethod_ThenSetupFailedNamesTheClash()
    {
        Tag<int> one = new(), two = new();
        var ex = Xunit.Assert.Throws<SetupFailed>(()
            => When(_ => The(one) + The(two)).Then().Result.Is(0));
        ex.Message.Does()
            .StartWith($"Two tags are named '{nameof(GivenTwoTagsDeclaredInAMethod_ThenSetupFailedNamesTheClash)}'.")
            .and.Contain("Name each one where it is declared");
    }

    /// <summary>Different types are no defence: the reader still sees two rows labelled alike.</summary>
    [Fact]
    public void GivenTagsOfDifferentTypesShareAName_ThenSetupFailedAllTheSame()
    {
        Tag<int> number = new("shared");
        Tag<string> text = new("shared");
        Xunit.Assert.Throws<SetupFailed>(()
            => Given(number).Is(1).And(text).Is("x").When(_ => The(number)).Then().Result.Is(1));
    }

    [Fact]
    public void GivenEachIsNamed_ThenBothAreAccepted()
    {
        Tag<int> low = new(nameof(low)), high = new(nameof(high));
        Given(low).Is(1).And(high).Is(2).When(_ => The(low) + The(high)).Then().Result.Is(3);
    }
}
