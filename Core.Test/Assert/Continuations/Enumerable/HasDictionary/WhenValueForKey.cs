using System.Collections.Frozen;
using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasDictionary;

public class WhenValueForKey : Spec
{
    private static Dictionary<string, int> Dict => new() { ["a"] = 1, ["b"] = 2 };

    [Fact]
    public void GivenPresentKey()
    {
        var dict = Dict;
        dict.Has("a").that.Is(1);
        Specification.Is("Dict has value for key \"a\" that is 1");
    }

    [Fact]
    public void GivenMissingKey()
    {
        var dict = Dict;
        var ex = Xunit.Assert.Throws<XunitException>(() => dict.Has("c"));
        ex.Message.Is("Expected dict to have value for key \"c\" but found [[a, 1], [b, 2]]");
    }

    [Fact]
    public void GivenContinuedOnDictionary()
    {
        var dict = Dict;
        dict.Has("a").and.Key("b");
    }

    [Fact]
    public void GivenValueAssertionChain()
    {
        var dict = Dict;
        dict.Has("b").that.Is().GreaterThan(1);
    }

    [Fact]
    public void GivenFrozenDictionary()
    {
        var dict = Dict.ToFrozenDictionary();
        dict.Has("a").that.Is(1);
    }

    [Fact]
    public void GivenCustomKeyComparer()
    {
        Dictionary<string, int> dict = new(StringComparer.OrdinalIgnoreCase) { ["a"] = 1 };
        dict.Has("A").that.Is(1);
    }
}
