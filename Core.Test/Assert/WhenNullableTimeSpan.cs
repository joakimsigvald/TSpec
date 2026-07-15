using TSpec.Assert;

namespace TSpec.Test.Assert;

public class WhenNullableTimeSpan : Spec<TimeSpan?>
{
    [Fact]
    public void IsSame()
    {
        When(_ => A(_)).Then().Result.Is(The<TimeSpan?>())
            .and.not.Null();
        Specification.Is(
            """
            When a _
            Then Result is the TimeSpan?
                and not null
            """);
    }

    [Fact]
    public void IsSameNotNullable()
    {
        When(_ => A(_)).Then().Result.Is(The<TimeSpan?>()!.Value)
            .and.CloseTo(The<TimeSpan?>()!.Value, TimeSpan.Zero);
        Specification.Is(
            """
            When a _
            Then Result is the TimeSpan?'s Value
                and close to the TimeSpan?'s Value
            """);
    }

    [Fact]
    public void IsNull()
    {
        When(_ => (TimeSpan?)null).Then().Result.Is().Null();
        Specification.Is(
            """
            When (TimeSpan?)null
            Then Result is null
            """);
    }
}