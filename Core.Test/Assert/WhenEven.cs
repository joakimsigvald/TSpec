using TSpec.Assert;

namespace TSpec.Test.Assert;

public class WhenEven : Spec<int>
{
    [Fact]
    public void GivenIsEvenFail()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => When(_ => 1).Then().Result.Is().Even());
        ex.HasMessage("Expected Result to be even but found 1", 
            """
            When 1
            Then Result is even
            """);
    }

    [Fact]
    public void GivenIsEven()
    {
        When(_ => 2).Then().Result.Is().Even();
        Specification.Is("""
            When 2
            Then Result is even
            """);
    }

    [Fact]
    public void GivenIsNotEven()
    {
        When(_ => 1).Then().Result.Is().not.Even();
        Specification.Is("""
            When 1
            Then Result is not even
            """);
    }

    [Fact]
    public void GivenIsNotEvenFail()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => When(_ => 2).Then().Result.Is().not.Even());
        ex.HasMessage("Expected Result to not be even but found 2",
            """
            When 2
            Then Result is not even
            """);
    }
}