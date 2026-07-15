using TSpec.Assert;

namespace TSpec.Test.Assert;

public class WhenDictionary : Spec<Dictionary<string, int>>
{
    [Fact]
    public void IsEqual()
    {
        var pairs = new[] { ("a", 1), ("b", 2) };
        var dict1 = pairs.ToDictionary(p => p.Item1, p => p.Item2);
        var dict2 = pairs.ToDictionary(p => p.Item1, p => p.Item2);
        dict1.Is().EqualTo(dict2);
        Specification.Is("Dict1 is equal to dict2");
    }
}