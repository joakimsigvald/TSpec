using TSpec.Assert;
using TSpec.Internal.TestData.Generation;
using TSpec.Internal.TestData.Generation.Strategies;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenDateTimeStartingAt : Spec<DateTime>
{
    private static readonly DateTime _start = new(2024, 5, 1);

    public WhenDateTimeStartingAt() => Using<DateTime>().From<DateTime>().StartingAt(_start);

    [Fact]
    public void ThenGenerateFromStartSpacedOneDay()
        => Three<DateTime>().Is().EqualTo([_start, _start.AddDays(1), _start.AddDays(2)]);
}

public class WhenDateTimeStartingAtSpaced : Spec<DateTime>
{
    private static readonly DateTime _start = new(2024, 5, 1);

    public WhenDateTimeStartingAtSpaced()
        => Using<DateTime>().From<DateTime>().StartingAt(_start).Spaced(TimeSpan.FromHours(6));

    [Fact]
    public void ThenGenerateSequence()
        => Three<DateTime>().Is().EqualTo([_start, _start.AddHours(6), _start.AddHours(12)]);
}

public class WhenDateTimeSpacedOnly : Spec<DateTime>
{
    public WhenDateTimeSpacedOnly() => Using<DateTime>().From<DateTime>().Spaced(TimeSpan.FromDays(7));

    [Fact]
    public void ThenStartAtEpoch()
        => Two<DateTime>().Is().EqualTo([PrimitiveStrategy.Epoch, PrimitiveStrategy.Epoch.AddDays(7)]);
}

public class WhenDateTimeSpacedThenStartingAt : Spec<DateTime>
{
    private static readonly DateTime _start = new(2024, 5, 1);

    public WhenDateTimeSpacedThenStartingAt()
        => Using<DateTime>().From<DateTime>().Spaced(TimeSpan.FromDays(10)).StartingAt(_start);

    [Fact]
    public void ThenBothApply()
        => Three<DateTime>().Is().EqualTo([_start, _start.AddDays(10), _start.AddDays(20)]);
}

public class WhenDateTimeSpacedByLambda : Spec<DateTime>
{
    private static readonly DateTime _start = new(2024, 5, 1);

    public WhenDateTimeSpacedByLambda()
        => Using<DateTime>().From<DateTime>().StartingAt(_start).Spaced(d => d.AddMonths(1));

    [Fact]
    public void ThenApplyLambda()
        => Three<DateTime>().Is().EqualTo([_start, _start.AddMonths(1), _start.AddMonths(2)]);
}

public class WhenDateTimeSpacedByIndexedLambda : Spec<DateTime>
{
    private static readonly DateTime _start = new(2024, 5, 1);

    public WhenDateTimeSpacedByIndexedLambda()
        => Using<DateTime>().From<DateTime>().StartingAt(_start).Spaced((_, i) => _start.AddDays(i * i));

    [Fact]
    public void ThenGenerateSquares()
        => Three<DateTime>().Is().EqualTo([_start, _start.AddDays(1), _start.AddDays(4)]);
}

public class WhenDateTimeSpacedDescending : Spec<DateTime>
{
    private static readonly DateTime _start = new(2024, 5, 1);

    public WhenDateTimeSpacedDescending()
        => Using<DateTime>().From<DateTime>().StartingAt(_start).Spaced(TimeSpan.FromDays(-1));

    [Fact]
    public void ThenGenerateDescending()
        => Three<DateTime>().Is().EqualTo([_start, _start.AddDays(-1), _start.AddDays(-2)]);
}

public class WhenDateTimeSequenceLoops : Spec<DateTime>
{
    private static readonly DateTime _start = new(2024, 5, 1);

    public WhenDateTimeSequenceLoops()
        => Using<DateTime>().From<DateTime>().StartingAt(_start).Spaced(d => d);

    [Fact]
    public void ThenThrowValuesExhaustedOnFirstDuplicate()
    {
        Any<DateTime>().Is(_start);
        Xunit.Assert.Throws<ValuesExhausted>(() => Any<DateTime>());
    }
}

public class WhenDateTimeSequencePassesMaxValue : Spec<DateTime>
{
    public WhenDateTimeSequencePassesMaxValue()
        => Using<DateTime>().From<DateTime>().StartingAt(DateTime.MaxValue);

    [Fact]
    public void ThenThrowValuesExhausted()
    {
        Any<DateTime>().Is(DateTime.MaxValue);
        Xunit.Assert.Throws<ValuesExhausted>(() => Any<DateTime>());
    }
}

public class WhenDateTimeSpacedZero : Spec<DateTime>
{
    [Fact]
    public void ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() => Using<DateTime>().From<DateTime>().Spaced(TimeSpan.Zero));
}

public class WhenDateTimeSequenceSpecification : Spec<MyService, int>
{
    private static readonly DateTime _start = new(2024, 5, 1);
    private static readonly TimeSpan _step = TimeSpan.FromDays(10);

    public WhenDateTimeSequenceSpecification() => When(_ => _.GetNextId());

    [Fact]
    public void ThenMentionSequenceAndScope()
    {
        Using<DateTime>(For.Input).From<DateTime>().StartingAt(_start).Spaced(_step)
            .Then().Result.Is().GreaterThan(0);
        Specification.Is(
            """
            Using DateTime from DateTime starting at _start spaced _step for Input
            When _.GetNextId()
            Then Result is greater than 0
            """
            );
    }
}
