using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasDictionary;

public class WhenValue : Spec
{
    private static Dictionary<string, int> Dict => new() { ["a"] = 1, ["b"] = 2 };

    [Fact]
    public void GivenPresentValue()
    {
        var dict = Dict;
        dict.Has().Value(1);
        Specification.Is("Dict has value 1");
    }

    [Fact]
    public void GivenMissingValue()
    {
        var dict = Dict;
        var ex = Xunit.Assert.Throws<XunitException>(() => dict.Has().Value(3));
        ex.Message.Is("Expected dict to have value 3 but found [[a, 1], [b, 2]]");
    }

    [Fact]
    public void GivenNoWithMissingValue()
    {
        var dict = Dict;
        dict.Has().no.Value(3);
        Specification.Is("Dict has no value 3");
    }

    [Fact]
    public void GivenNoWithPresentValue()
    {
        var dict = Dict;
        var ex = Xunit.Assert.Throws<XunitException>(() => dict.Has().no.Value(1));
        ex.Message.Is("Expected dict to not have value 1 but found [[a, 1], [b, 2]]");
    }

    [Fact]
    public void GivenChainedKeyAndValue()
    {
        var dict = Dict;
        dict.Has().Key("a").and.Value(2).and.no.Value(3);
    }
}
