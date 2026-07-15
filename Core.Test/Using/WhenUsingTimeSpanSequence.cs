using TSpec.Assert;
using TSpec.Internal.TestData.Generation;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenTimeSpanStartingAt : Spec<TimeSpan>
{
    private static readonly TimeSpan _start = TimeSpan.FromHours(8);

    public WhenTimeSpanStartingAt() => Using<TimeSpan>().From<TimeSpan>().StartingAt(_start);

    [Fact]
    public void ThenGenerateFromStartSpacedOneDay()
        => Three<TimeSpan>().Is().EqualTo([_start, _start + TimeSpan.FromDays(1), _start + TimeSpan.FromDays(2)]);
}

public class WhenTimeSpanStartingAtSpaced : Spec<TimeSpan>
{
    private static readonly TimeSpan _start = TimeSpan.FromHours(8);

    public WhenTimeSpanStartingAtSpaced()
        => Using<TimeSpan>().From<TimeSpan>().StartingAt(_start).Spaced(TimeSpan.FromMinutes(30));

    [Fact]
    public void ThenGenerateSequence()
        => Three<TimeSpan>().Is().EqualTo([_start, _start + TimeSpan.FromMinutes(30), _start + TimeSpan.FromHours(1)]);
}

public class WhenTimeSpanSpacedOnly : Spec<TimeSpan>
{
    public WhenTimeSpanSpacedOnly() => Using<TimeSpan>().From<TimeSpan>().Spaced(TimeSpan.FromHours(12));

    [Fact]
    public void ThenStartAtOneDay()
        => Two<TimeSpan>().Is().EqualTo([TimeSpan.FromDays(1), TimeSpan.FromHours(36)]);
}

public class WhenTimeSpanSpacedThenStartingAt : Spec<TimeSpan>
{
    private static readonly TimeSpan _start = TimeSpan.FromHours(8);

    public WhenTimeSpanSpacedThenStartingAt()
        => Using<TimeSpan>().From<TimeSpan>().Spaced(TimeSpan.FromDays(10)).StartingAt(_start);

    [Fact]
    public void ThenBothApply()
        => Three<TimeSpan>().Is().EqualTo([_start, _start + TimeSpan.FromDays(10), _start + TimeSpan.FromDays(20)]);
}

public class WhenTimeSpanSpacedByLambda : Spec<TimeSpan>
{
    private static readonly TimeSpan _start = TimeSpan.FromHours(8);

    public WhenTimeSpanSpacedByLambda()
        => Using<TimeSpan>().From<TimeSpan>().StartingAt(_start).Spaced(t => t * 2);

    [Fact]
    public void ThenApplyLambda()
        => Three<TimeSpan>().Is().EqualTo([_start, _start * 2, _start * 4]);
}

public class WhenTimeSpanSpacedByIndexedLambda : Spec<TimeSpan>
{
    private static readonly TimeSpan _start = TimeSpan.FromHours(8);

    public WhenTimeSpanSpacedByIndexedLambda()
        => Using<TimeSpan>().From<TimeSpan>().StartingAt(_start).Spaced((_, i) => _start + TimeSpan.FromHours(i * i));

    [Fact]
    public void ThenGenerateSquares()
        => Three<TimeSpan>().Is().EqualTo([_start, _start + TimeSpan.FromHours(1), _start + TimeSpan.FromHours(4)]);
}

public class WhenTimeSpanSpacedDescending : Spec<TimeSpan>
{
    private static readonly TimeSpan _start = TimeSpan.FromHours(8);

    public WhenTimeSpanSpacedDescending()
        => Using<TimeSpan>().From<TimeSpan>().StartingAt(_start).Spaced(TimeSpan.FromHours(-1));

    [Fact]
    public void ThenGenerateDescending()
        => Three<TimeSpan>().Is().EqualTo([_start, _start - TimeSpan.FromHours(1), _start - TimeSpan.FromHours(2)]);
}

public class WhenTimeSpanSequenceLoops : Spec<TimeSpan>
{
    private static readonly TimeSpan _start = TimeSpan.FromHours(8);

    public WhenTimeSpanSequenceLoops()
        => Using<TimeSpan>().From<TimeSpan>().StartingAt(_start).Spaced(t => t);

    [Fact]
    public void ThenThrowValuesExhaustedOnFirstDuplicate()
    {
        Any<TimeSpan>().Is(_start);
        Xunit.Assert.Throws<ValuesExhausted>(() => Any<TimeSpan>());
    }
}

public class WhenTimeSpanSequencePassesMaxValue : Spec<TimeSpan>
{
    public WhenTimeSpanSequencePassesMaxValue()
        => Using<TimeSpan>().From<TimeSpan>().StartingAt(TimeSpan.MaxValue);

    [Fact]
    public void ThenThrowValuesExhausted()
    {
        Any<TimeSpan>().Is(TimeSpan.MaxValue);
        Xunit.Assert.Throws<ValuesExhausted>(() => Any<TimeSpan>());
    }
}

public class WhenTimeSpanSpacedZero : Spec<TimeSpan>
{
    [Fact]
    public void ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() => Using<TimeSpan>().From<TimeSpan>().Spaced(TimeSpan.Zero));
}

public class WhenTimeSpanSequenceSpecification : Spec<MyService, int>
{
    private static readonly TimeSpan _start = TimeSpan.FromHours(8);
    private static readonly TimeSpan _step = TimeSpan.FromMinutes(30);

    public WhenTimeSpanSequenceSpecification() => When(_ => _.GetNextId());

    [Fact]
    public void ThenMentionSequenceAndScope()
    {
        Using<TimeSpan>(For.Input).From<TimeSpan>().StartingAt(_start).Spaced(_step)
            .Then().Result.Is().GreaterThan(0);
        Specification.Is(
            """
            Using TimeSpan from TimeSpan starting at _start spaced _step for Input
            When _.GetNextId()
            Then Result is greater than 0
            """
            );
    }
}
