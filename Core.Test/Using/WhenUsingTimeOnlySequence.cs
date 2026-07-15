using TSpec.Assert;
using TSpec.Internal.TestData.Generation;
using TSpec.Internal.TestData.Generation.Strategies;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenTimeOnlyStartingAt : Spec<TimeOnly>
{
    private static readonly TimeOnly _start = new(8, 30);

    public WhenTimeOnlyStartingAt() => Using<TimeOnly>().From<TimeOnly>().StartingAt(_start);

    [Fact]
    public void ThenGenerateFromStartSpacedOneHour()
        => Three<TimeOnly>().Is().EqualTo([_start, _start.AddHours(1), _start.AddHours(2)]);
}

public class WhenTimeOnlyStartingAtSpaced : Spec<TimeOnly>
{
    private static readonly TimeOnly _start = new(8, 30);

    public WhenTimeOnlyStartingAtSpaced()
        => Using<TimeOnly>().From<TimeOnly>().StartingAt(_start).Spaced(TimeSpan.FromMinutes(15));

    [Fact]
    public void ThenGenerateSequence()
        => Three<TimeOnly>().Is().EqualTo([_start, _start.AddMinutes(15), _start.AddMinutes(30)]);
}

public class WhenTimeOnlySpacedOnly : Spec<TimeOnly>
{
    private static readonly TimeOnly _noon = TimeOnly.FromDateTime(PrimitiveStrategy.Epoch);

    public WhenTimeOnlySpacedOnly() => Using<TimeOnly>().From<TimeOnly>().Spaced(TimeSpan.FromMinutes(90));

    [Fact]
    public void ThenStartAtNoon()
        => Two<TimeOnly>().Is().EqualTo([_noon, _noon.AddMinutes(90)]);
}

public class WhenTimeOnlySequenceWrapsAtMidnight : Spec<TimeOnly>
{
    public WhenTimeOnlySequenceWrapsAtMidnight()
        => Using<TimeOnly>().From<TimeOnly>().Spaced(TimeSpan.FromHours(8));

    [Fact]
    public void ThenYieldAllUniqueValuesBeforeThrowing()
    {
        Three<TimeOnly>().Is().EqualTo([new(12, 0), new(20, 0), new(4, 0)]);
        Xunit.Assert.Throws<ValuesExhausted>(() => Any<TimeOnly>());
    }
}

public class WhenTimeOnlySpacedByLambda : Spec<TimeOnly>
{
    private static readonly TimeOnly _start = new(8, 30);

    public WhenTimeOnlySpacedByLambda()
        => Using<TimeOnly>().From<TimeOnly>().StartingAt(_start).Spaced(t => t.AddMinutes(90));

    [Fact]
    public void ThenApplyLambda()
        => Three<TimeOnly>().Is().EqualTo([_start, _start.AddMinutes(90), _start.AddMinutes(180)]);
}

public class WhenTimeOnlySpacedZero : Spec<TimeOnly>
{
    [Fact]
    public void ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() => Using<TimeOnly>().From<TimeOnly>().Spaced(TimeSpan.Zero));
}

public class WhenTimeOnlySequenceSpecification : Spec<MyService, int>
{
    private static readonly TimeOnly _start = new(8, 30);
    private static readonly TimeSpan _step = TimeSpan.FromMinutes(15);

    public WhenTimeOnlySequenceSpecification() => When(_ => _.GetNextId());

    [Fact]
    public void ThenMentionSequence()
    {
        Using<TimeOnly>().From<TimeOnly>().StartingAt(_start).Spaced(_step)
            .Then().Result.Is().GreaterThan(0);
        Specification.Is(
            """
            Using TimeOnly from TimeOnly starting at _start spaced _step
            When _.GetNextId()
            Then Result is greater than 0
            """
            );
    }
}
