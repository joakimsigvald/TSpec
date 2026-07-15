using TSpec.Assert;
using TSpec.Internal.TestData.Generation;
using TSpec.Internal.TestData.Generation.Strategies;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenDateTimeOffsetStartingAt : Spec<DateTimeOffset>
{
    private static readonly DateTimeOffset _start = new(2024, 5, 1, 0, 0, 0, TimeSpan.FromHours(2));

    public WhenDateTimeOffsetStartingAt() => Using<DateTimeOffset>().From<DateTimeOffset>().StartingAt(_start);

    [Fact]
    public void ThenGenerateFromStartSpacedOneDay()
        => Three<DateTimeOffset>().Is().EqualTo([_start, _start.AddDays(1), _start.AddDays(2)]);
}

public class WhenDateTimeOffsetStartingAtSpaced : Spec<DateTimeOffset>
{
    private static readonly DateTimeOffset _start = new(2024, 5, 1, 0, 0, 0, TimeSpan.FromHours(2));

    public WhenDateTimeOffsetStartingAtSpaced()
        => Using<DateTimeOffset>().From<DateTimeOffset>().StartingAt(_start).Spaced(TimeSpan.FromHours(6));

    [Fact]
    public void ThenGenerateSequence()
        => Three<DateTimeOffset>().Is().EqualTo([_start, _start.AddHours(6), _start.AddHours(12)]);
}

public class WhenDateTimeOffsetSpacedOnly : Spec<DateTimeOffset>
{
    private static readonly DateTimeOffset _epoch = new(PrimitiveStrategy.Epoch);

    public WhenDateTimeOffsetSpacedOnly()
        => Using<DateTimeOffset>().From<DateTimeOffset>().Spaced(TimeSpan.FromDays(7));

    [Fact]
    public void ThenStartAtEpoch()
        => Two<DateTimeOffset>().Is().EqualTo([_epoch, _epoch.AddDays(7)]);
}

public class WhenDateTimeOffsetSpacedByLambda : Spec<DateTimeOffset>
{
    private static readonly DateTimeOffset _start = new(2024, 5, 1, 0, 0, 0, TimeSpan.FromHours(2));

    public WhenDateTimeOffsetSpacedByLambda()
        => Using<DateTimeOffset>().From<DateTimeOffset>().StartingAt(_start).Spaced(d => d.AddMonths(1));

    [Fact]
    public void ThenApplyLambda()
        => Three<DateTimeOffset>().Is().EqualTo([_start, _start.AddMonths(1), _start.AddMonths(2)]);
}

public class WhenDateTimeOffsetSequencePassesMaxValue : Spec<DateTimeOffset>
{
    public WhenDateTimeOffsetSequencePassesMaxValue()
        => Using<DateTimeOffset>().From<DateTimeOffset>().StartingAt(DateTimeOffset.MaxValue);

    [Fact]
    public void ThenThrowValuesExhausted()
    {
        Any<DateTimeOffset>().Is(DateTimeOffset.MaxValue);
        Xunit.Assert.Throws<ValuesExhausted>(() => Any<DateTimeOffset>());
    }
}

public class WhenDateTimeOffsetSpacedZero : Spec<DateTimeOffset>
{
    [Fact]
    public void ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(
            () => Using<DateTimeOffset>().From<DateTimeOffset>().Spaced(TimeSpan.Zero));
}

public class WhenDateTimeOffsetSequenceSpecification : Spec<MyService, int>
{
    private static readonly DateTimeOffset _start = new(2024, 5, 1, 0, 0, 0, TimeSpan.FromHours(2));
    private static readonly TimeSpan _step = TimeSpan.FromDays(10);

    public WhenDateTimeOffsetSequenceSpecification() => When(_ => _.GetNextId());

    [Fact]
    public void ThenMentionSequence()
    {
        Using<DateTimeOffset>().From<DateTimeOffset>().StartingAt(_start).Spaced(_step)
            .Then().Result.Is().GreaterThan(0);
        Specification.Is(
            """
            Using DateTimeOffset from DateTimeOffset starting at _start spaced _step
            When _.GetNextId()
            Then Result is greater than 0
            """
            );
    }
}
