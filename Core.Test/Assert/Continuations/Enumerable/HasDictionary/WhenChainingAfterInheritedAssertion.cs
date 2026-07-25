using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasDictionary;

/// <summary>
/// The enumerable assertions inherited by dictionaries and strings continue with the
/// specific vocabulary, not the plain enumerable one.
/// </summary>
public class WhenChainingAfterInheritedAssertion : Spec
{
    private static Dictionary<string, int> Dict => new() { ["a"] = 1, ["b"] = 2 };

    [Fact]
    public void GivenCountThenKey()
    {
        var dict = Dict;
        dict.Has().Count(2).and.Key("a");
        Specification.Is("""
            Dict has count 2
                and key "a"
            """);
    }

    [Fact]
    public void GivenTwoItemsThenNoKey()
    {
        var dict = Dict;
        dict.Has().TwoItems().and.no.Key("c");
    }

    [Fact]
    public void GivenCountThenValue()
    {
        var dict = Dict;
        dict.Has().Count().AtLeast(1).and.Value(2);
    }

    [Fact]
    public void GivenStringCountThenLength()
    {
        var text = "abc";
        text.Has().Count(3).and.Length().AtLeast(2);
    }
}
