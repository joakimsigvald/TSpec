using TSpec.Assert;
using TSpec.Internal.TestData.Generation;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenStartingAt : Spec<int>
{
    public WhenStartingAt() => Using<int>().From<int>().StartingAt(100);

    [Fact]
    public void ThenGenerateFromStart() => Three<int>().Is().EqualTo([100, 101, 102]);
}

public class WhenStartingAtSpaced : Spec<int>
{
    public WhenStartingAtSpaced() => Using<int>().From<int>().StartingAt(10).Spaced(5);

    [Fact]
    public void ThenGenerateSequence() => Three<int>().Is().EqualTo([10, 15, 20]);
}

public class WhenSpacedOnly : Spec<int>
{
    public WhenSpacedOnly() => Using<int>().From<int>().Spaced(10);

    [Fact]
    public void ThenStartAtOne() => Three<int>().Is().EqualTo([1, 11, 21]);
}

public class WhenSpacedThenStartingAt : Spec<int>
{
    public WhenSpacedThenStartingAt() => Using<int>().From<int>().Spaced(10).StartingAt(5);

    [Fact]
    public void ThenBothApply() => Three<int>().Is().EqualTo([5, 15, 25]);
}

public class WhenSpacedByLambda : Spec<int>
{
    public WhenSpacedByLambda() => Using<int>().From<int>().StartingAt(1).Spaced(i => i * 2);

    [Fact]
    public void ThenApplyLambda() => Three<int>().Is().EqualTo([1, 2, 4]);
}

public class WhenSpacedByIndexedLambda : Spec<int>
{
    public WhenSpacedByIndexedLambda() => Using<int>().From<int>().StartingAt(0).Spaced((_, i) => i * i);

    [Fact]
    public void ThenGenerateSquares() => Three<int>().Is().EqualTo([0, 1, 4]);
}

public class WhenSpacedByAlternatingIndexedLambda : Spec<int>
{
    public WhenSpacedByAlternatingIndexedLambda()
        => Using<int>().From<int>().StartingAt(0).Spaced((_, i) => i % 2 == 0 ? i : -i);

    [Fact]
    public void ThenGenerateAlternatingSeries() => Four<int>().Is().EqualTo([0, -1, 2, -3]);
}

public class WhenSpacedDescending : Spec<int>
{
    public WhenSpacedDescending() => Using<int>().From<int>().StartingAt(3).Spaced(-1);

    [Fact]
    public void ThenGenerateDescending() => Three<int>().Is().EqualTo([3, 2, 1]);
}

public class WhenConvertedSequence : Spec<int>
{
    public WhenConvertedSequence() => Using<int>().From((byte b) => b + 100).StartingAt((byte)5);

    [Fact]
    public void ThenConvertSequenceValues() => Three<int>().Is().EqualTo([105, 106, 107]);
}

public class WhenSequenceWrapsAroundTypeBoundary : Spec<byte>
{
    public WhenSequenceWrapsAroundTypeBoundary() => Using<byte>().From<byte>().StartingAt((byte)254);

    [Fact]
    public void ThenContinueFromMinValue() => Three<byte>().Is().EqualTo([254, 255, 0]);
}

public class WhenSequenceExhausted : Spec<byte>
{
    public WhenSequenceExhausted() => Using<byte>().From<byte>().StartingAt((byte)254);

    [Fact]
    public void ThenYieldAllUniqueValuesBeforeThrowing()
    {
        Enumerable.Range(1, 256).Select(n => Any<byte>()).Is().Distinct();
        Xunit.Assert.Throws<ValuesExhausted>(() => Any<byte>());
    }
}

public class WhenSequenceLoops : Spec<int>
{
    public WhenSequenceLoops() => Using<int>().From<int>().StartingAt(1).Spaced(i => 3 - i);

    [Fact]
    public void ThenThrowValuesExhaustedOnFirstDuplicate()
    {
        Any<int>().Is(1);
        Any<int>().Is(2);
        Xunit.Assert.Throws<ValuesExhausted>(() => Any<int>());
    }
}

public class WhenSpacingVanishesInFloatingPointPrecision : Spec<double>
{
    public WhenSpacingVanishesInFloatingPointPrecision()
        => Using<double>().From<double>().StartingAt(1e17).Spaced(1d);

    [Fact]
    public void ThenThrowValuesExhaustedOnStall()
    {
        Any<double>().Is(1e17);
        Xunit.Assert.Throws<ValuesExhausted>(() => Any<double>());
    }
}

public class WhenFromSameTypeWithoutSequence : Spec<int>
{
    public WhenFromSameTypeWithoutSequence() => Using<int>().From<int>();

    [Fact]
    public void ThenThrowSetupFailed() => Xunit.Assert.Throws<SetupFailed>(() => Any<int>());
}

public class WhenSpacedZero : Spec<int>
{
    [Fact]
    public void ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() => Using<int>().From<int>().Spaced(0));
}

public class WhenSpacedTwice : Spec<int>
{
    [Fact]
    public void ThenThrowSetupFailed()
    {
        var continuation = Using<int>().From<int>().Spaced(2);
        Xunit.Assert.Throws<SetupFailed>(() => continuation.Spaced(3));
    }
}

public class WhenStartingAtTwice : Spec<int>
{
    [Fact]
    public void ThenThrowSetupFailed()
    {
        var continuation = Using<int>().From<int>().StartingAt(1);
        Xunit.Assert.Throws<SetupFailed>(() => continuation.StartingAt(2));
    }
}

public class WhenSequenceSpecification : Spec<MyService, int>
{
    public WhenSequenceSpecification() => When(_ => _.GetNextId());

    [Fact]
    public void ThenMentionSequenceAndScope()
    {
        Using<int>(For.Subject).From<int>().StartingAt(100).Spaced(10).Then().Result.Is(100);
        Specification.Is(
            """
            Using int from int starting at 100 spaced 10 for Subject
            When _.GetNextId()
            Then Result is 100
            """
            );
    }
}
