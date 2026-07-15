using TSpec.Assert;
using TSpec.Internal.TestData.Generation;
using TSpec.Internal.TestData.Generation.Strategies;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenDateOnlyStartingAt : Spec<DateOnly>
{
    private static readonly DateOnly _start = new(2024, 5, 1);

    public WhenDateOnlyStartingAt() => Using<DateOnly>().From<DateOnly>().StartingAt(_start);

    [Fact]
    public void ThenGenerateFromStartSpacedOneDay()
        => Three<DateOnly>().Is().EqualTo([_start, _start.AddDays(1), _start.AddDays(2)]);
}

public class WhenDateOnlyStartingAtSpaced : Spec<DateOnly>
{
    private static readonly DateOnly _start = new(2024, 5, 1);

    public WhenDateOnlyStartingAtSpaced()
        => Using<DateOnly>().From<DateOnly>().StartingAt(_start).Spaced(7);

    [Fact]
    public void ThenGenerateSequence()
        => Three<DateOnly>().Is().EqualTo([_start, _start.AddDays(7), _start.AddDays(14)]);
}

public class WhenDateOnlySpacedOnly : Spec<DateOnly>
{
    private static readonly DateOnly _epoch = DateOnly.FromDateTime(PrimitiveStrategy.Epoch);

    public WhenDateOnlySpacedOnly() => Using<DateOnly>().From<DateOnly>().Spaced(7);

    [Fact]
    public void ThenStartAtEpochDate()
        => Two<DateOnly>().Is().EqualTo([_epoch, _epoch.AddDays(7)]);
}

public class WhenDateOnlySpacedDescending : Spec<DateOnly>
{
    private static readonly DateOnly _start = new(2024, 5, 1);

    public WhenDateOnlySpacedDescending()
        => Using<DateOnly>().From<DateOnly>().StartingAt(_start).Spaced(-1);

    [Fact]
    public void ThenGenerateDescending()
        => Three<DateOnly>().Is().EqualTo([_start, _start.AddDays(-1), _start.AddDays(-2)]);
}

public class WhenDateOnlySpacedByLambda : Spec<DateOnly>
{
    private static readonly DateOnly _start = new(2024, 5, 1);

    public WhenDateOnlySpacedByLambda()
        => Using<DateOnly>().From<DateOnly>().StartingAt(_start).Spaced(d => d.AddMonths(1));

    [Fact]
    public void ThenApplyLambda()
        => Three<DateOnly>().Is().EqualTo([_start, _start.AddMonths(1), _start.AddMonths(2)]);
}

public class WhenDateOnlySequencePassesMaxValue : Spec<DateOnly>
{
    public WhenDateOnlySequencePassesMaxValue()
        => Using<DateOnly>().From<DateOnly>().StartingAt(DateOnly.MaxValue);

    [Fact]
    public void ThenThrowValuesExhausted()
    {
        Any<DateOnly>().Is(DateOnly.MaxValue);
        Xunit.Assert.Throws<ValuesExhausted>(() => Any<DateOnly>());
    }
}

public class WhenDateOnlySpacedZero : Spec<DateOnly>
{
    [Fact]
    public void ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() => Using<DateOnly>().From<DateOnly>().Spaced(0));
}

public class WhenDateOnlySequenceSpecification : Spec<MyService, int>
{
    private static readonly DateOnly _start = new(2024, 5, 1);

    public WhenDateOnlySequenceSpecification() => When(_ => _.GetNextId());

    [Fact]
    public void ThenMentionSequence()
    {
        Using<DateOnly>().From<DateOnly>().StartingAt(_start).Spaced(7)
            .Then().Result.Is().GreaterThan(0);
        Specification.Is(
            """
            Using DateOnly from DateOnly starting at _start spaced 7
            When _.GetNextId()
            Then Result is greater than 0
            """
            );
    }
}
