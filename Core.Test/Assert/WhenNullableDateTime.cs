using TSpec.Assert;

namespace TSpec.Test.Assert;

public class WhenNullableDateTime : Spec<DateTime?>
{
    [Fact]
    public void IsSame()
    {
        When(_ => A(_)).Then().Result.Is(The<DateTime?>())
            .and.not.Null();
        Specification.Is(
            """
            When a _
            Then Result is the DateTime?
                and not null
            """);
    }

    [Fact]
    public void IsSameNotNullable()
    {
        When(_ => A(_)).Then().Result.Is(The<DateTime?>()!.Value)
            .and.CloseTo(The<DateTime?>()!.Value, TimeSpan.Zero);
        Specification.Is(
            """
            When a _
            Then Result is the DateTime?'s Value
                and close to the DateTime?'s Value
            """);
    }

    [Fact]
    public void IsNull()
    {
        When(_ => (DateTime?)null).Then().Result.Is().Null();
        Specification.Is(
            """
            When (DateTime?)null
            Then Result is null
            """);
    }

    [Fact]
    public void IsBeforeEtc()
    {
        Using((DateTime?)DateTime.Now).When(_ => A(_));
        Result.Is().Before(The<DateTime?>()!.Value.AddDays(1)).and.After(The<DateTime?>()!.Value.AddDays(-1));
        Result.Is().not.Before(The<DateTime?>()!.Value);
        Result.Is().not.After(The<DateTime?>()!.Value);
        Specification.Is(
            """
            Using (DateTime?)DateTime.Now
            When a _
            Then Result is before the DateTime?'s Value.AddDays(1)
                and after the DateTime?'s Value.AddDays(-1)
            Result is not before the DateTime?'s Value
            Result is not after the DateTime?'s Value
            """);
    }
}