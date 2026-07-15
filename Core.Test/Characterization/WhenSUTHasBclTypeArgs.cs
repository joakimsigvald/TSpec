using TSpec.Assert;

namespace TSpec.Test.Characterization;

public class WhenSUTHasUriArg : Spec<UriHolder, Uri>
{
    public WhenSUTHasUriArg() => When(_ => _.Get());

    [Fact]
    public void Then_UriIsGenerated_AndNonNull()
    {
        Then().Result.Is().Not(null!);
        Specification.Is(
            """
            When _.Get()
            Then Result is not null
            """);
    }
}

public class WhenSUTHasTimeSpanArg : Spec<TimeSpanHolder, TimeSpan>
{
    public WhenSUTHasTimeSpanArg() => When(_ => _.Get());

    [Fact]
    public void Then_TimeSpanIsGenerated_AndNonZero()
    {
        Then().Result.Is().Not(TimeSpan.Zero);
        Specification.Is(
            """
            When _.Get()
            Then Result is not TimeSpan.Zero
            """);
    }
}

public class WhenSUTHasDateTimeOffsetArg : Spec<DateTimeOffsetHolder, DateTimeOffset>
{
    public WhenSUTHasDateTimeOffsetArg() => When(_ => _.Get());

    [Fact]
    public void Then_DateTimeOffsetIsGenerated_AndNonDefault()
    {
        Then().Result.Is().Not(default(DateTimeOffset));
        Specification.Is(
            """
            When _.Get()
            Then Result is not default DateTimeOffset
            """);
    }
}

public class WhenSUTHasGuidArg : Spec<GuidHolder, Guid>
{
    public WhenSUTHasGuidArg() => When(_ => _.Get());

    [Fact]
    public void Then_GuidIsGenerated_NonEmpty()
    {
        Then().Result.Is().Not(Guid.Empty);
        Specification.Is(
            """
            When _.Get()
            Then Result is not Guid.Empty
            """);
    }
}

public class WhenSUTHasMixedBclArgs : Spec<MixedBclHolder, string>
{
    public WhenSUTHasMixedBclArgs() => When(_ => _.Describe());

    [Fact]
    public void Then_AllArgsAreInjected_AndDescriptionIsNonNull()
    {
        Then().Result.Is().Not(null!);
        Specification.Is(
            """
            When _.Describe()
            Then Result is not null
            """);
    }
}

public class UriHolder(Uri uri)
{
    private readonly Uri _uri = uri;
    public Uri Get() => _uri;
}

public class TimeSpanHolder(TimeSpan span)
{
    private readonly TimeSpan _span = span;
    public TimeSpan Get() => _span;
}

public class DateTimeOffsetHolder(DateTimeOffset dto)
{
    private readonly DateTimeOffset _dto = dto;
    public DateTimeOffset Get() => _dto;
}

public class GuidHolder(Guid id)
{
    private readonly Guid _id = id;
    public Guid Get() => _id;
}

public class MixedBclHolder(Uri uri, TimeSpan span, DateTimeOffset dto, Guid id)
{
    public string Describe() => $"{uri};{span};{dto:O};{id}";
}
