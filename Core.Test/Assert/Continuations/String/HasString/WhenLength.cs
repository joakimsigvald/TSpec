using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.String.HasString;

public class WhenLength : StringSpec
{
    [Fact]
    public void GivenExactLength()
    {
        var name = "abc";
        name.Has().Length(3);
        Specification.Is("Name has length 3");
    }

    [Fact]
    public void GivenWrongLength()
    {
        var name = "abc";
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => name.Has().Length(5));
        ex.HasMessage("Expected name to have length 5 but found 3: \"abc\"");
    }

    [Fact]
    public void GivenNull()
    {
        string? name = null;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => name.Has().Length(5));
        ex.HasMessage("Expected name to have length 5 but found null");
    }

    [Fact]
    public void GivenAtLeast()
    {
        var name = "abc";
        name.Has().Length().AtLeast(2);
        Specification.Is("Name has length at least 2");
    }

    [Fact]
    public void GivenAtLeastFail()
    {
        var name = "abc";
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => name.Has().Length().AtLeast(5));
        ex.HasMessage("Expected name to have length at least 5 but found 3: \"abc\"");
    }

    [Fact]
    public void GivenAtMost()
    {
        var name = "abc";
        name.Has().Length().AtMost(5);
        Specification.Is("Name has length at most 5");
    }

    [Fact]
    public void GivenAtMostFail()
    {
        var name = "abc";
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => name.Has().Length().AtMost(2));
        ex.HasMessage("Expected name to have length at most 2 but found 3: \"abc\"");
    }

    [Fact]
    public void GivenInRange()
    {
        var name = "abc";
        name.Has().Length().InRange(2, 4);
        Specification.Is("Name has length between 2 and 4");
    }

    [Fact]
    public void GivenInvalidRange()
    {
        var name = "abc";
        Xunit.Assert.Throws<SetupFailed>(() => name.Has().Length().InRange(4, 2));
    }

    [Fact]
    public void GivenChainedStringAssertions()
    {
        var name = "abc";
        name.Has().Length(3).and.Is().Like("ABC");
        name.Has().Length().AtLeast(2).and.Does().StartWith("a");
    }

    [Fact]
    public void GivenInheritedEnumerableAssertions()
    {
        var name = "abc";
        name.Has().Count(3);
        name.Has().All(c => char.IsLetter(c));
    }
}
