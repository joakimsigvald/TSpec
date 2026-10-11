using TSpec.Assert;

namespace TSpec.Test.Assert.EitherOr;

public class WhenEitherIsEqualTo : Spec<int>
{
    [Fact]
    public void AndTheLastIsEqual()
    {
        When(_ => 3).Then().Result.Is().either.EqualTo(1)._.EqualTo(2).or.EqualTo(3);
        Specification.Is("""
            When 3
            Then Result is equal to 1, equal to 2
                or equal to 3
            """);
    }

    [Fact]
    public void AndNoneIsEqual()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => When(_ => 4).Then().Result.Is().either.EqualTo(1).or.EqualTo(3));
        ex.HasMessage("Expected Result to be equal to 1 or be equal to 3 but found 4");
    }
}

public class WhenEqualToOnEachType
{
    [Fact]
    public void GivenAString() => "b".Is().either.EqualTo("a").or.EqualTo("b");

    [Fact]
    public void GivenABool() => true.Is().either.EqualTo(false).or.EqualTo(true);

    [Fact]
    public void GivenADouble() => 0.5.Is().either.EqualTo(1.0).or.EqualTo(0.5);

    [Fact]
    public void GivenADate() => new DateOnly(2026, 10, 11).Is().either.EqualTo(default).or.EqualTo(new(2026, 10, 11));

    [Fact]
    public void GivenANullableInt() => ((int?)null).Is().either.EqualTo(1).or.EqualTo(null);

    [Fact]
    public void GivenANullableDate() => ((DateOnly?)null).Is().either.EqualTo(default(DateOnly)).or.EqualTo(null);

    [Fact]
    public void GivenANullableEnum()
        => ((DayOfWeek?)DayOfWeek.Monday).Is().either.EqualTo(null).or.EqualTo(DayOfWeek.Monday);
}
