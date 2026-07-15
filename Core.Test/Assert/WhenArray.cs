using TSpec.Assert;

namespace TSpec.Test.Assert;

public class WhenArray : Spec<int[]>
{
    [Fact]
    public void IsSameAs()
    {
        var values1 = new[] { 1, 2 };
        var values2 = values1;
        values1.Is(values2);
        Specification.Is("Values1 is values2");
    }

    [Fact]
    public void IsEqualButNotSame()
    {
        var values1 = new[] { 1, 2 };
        var values2 = values1.ToArray();
        values1.Is().Not(values2).but.EqualTo(values2);
        Specification.Is(
            """
            Values1 is not values2
                but equal to values2
            """);
    }

    [Fact]
    public void IsNotEqual()
    {
        var values1 = new[] { 1, 2 };
        var values2 = new[] { 1, 2, 3 };
        values1.Is().not.EqualTo(values2);
        Specification.Is("Values1 is not equal to values2");
    }

    [Fact]
    public void IsEmpty()
    {
        var values = Array.Empty<string>();
        values.Is().Empty();
        Specification.Is("Values is empty");
    }

    [Fact]
    public void IsNotEmpty()
    {
        var values = new[] { 1, 2 };
        values.Is().not.Empty();
        Specification.Is("Values is not empty");
    }

    [Fact]
    public void IsNotEmptyAndHasOneItem()
    {
        var values = new[] { 1 };
        values.Is().not.Empty().and.Has().OneItem(it => it == 1);
        Specification.Is(
            """
            Values is not empty
                and has one item it == 1
            """);
    }
}