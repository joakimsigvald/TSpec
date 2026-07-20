using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasDictionary;

public class WhenKey : Spec
{
    private static Dictionary<string, int> Dict => new() { ["a"] = 1, ["b"] = 2 };

    [Fact]
    public void GivenPresentKey()
    {
        var dict = Dict;
        dict.Has().Key("a");
        Specification.Is("Dict has key \"a\"");
    }

    [Fact]
    public void GivenMissingKey()
    {
        var dict = Dict;
        var ex = Xunit.Assert.Throws<XunitException>(() => dict.Has().Key("c"));
        ex.Message.Is("Expected dict to have key \"c\" but found [[a, 1], [b, 2]]");
    }

    [Fact]
    public void GivenNoWithMissingKey()
    {
        var dict = Dict;
        dict.Has().no.Key("c");
        Specification.Is("Dict has no key \"c\"");
    }

    [Fact]
    public void GivenNoWithPresentKey()
    {
        var dict = Dict;
        var ex = Xunit.Assert.Throws<XunitException>(() => dict.Has().no.Key("a"));
        ex.Message.Is("Expected dict to not have key \"a\" but found [[a, 1], [b, 2]]");
    }

    [Fact]
    public void GivenIntKeys()
    {
        Dictionary<int, string> dict = new() { [1] = "a" };
        dict.Has().Key(1).and.no.Key(2);
    }

    [Fact]
    public void GivenCustomKeyComparer()
    {
        Dictionary<string, int> dict = new(StringComparer.OrdinalIgnoreCase) { ["a"] = 1 };
        dict.Has().Key("A");
    }

    [Fact]
    public void GivenChainedKeys()
    {
        var dict = Dict;
        dict.Has().Key("a").and.Key("b");
    }

    [Fact]
    public void GivenChainedCount()
    {
        var dict = Dict;
        dict.Has().Key("a").and.Count(2);
    }
}
