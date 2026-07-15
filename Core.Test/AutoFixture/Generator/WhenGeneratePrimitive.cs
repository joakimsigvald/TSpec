using TSpec.Assert;

namespace TSpec.Test.AutoFixture.Generator;

public abstract class WhenGeneratePrimitive<TPrimitive>(int maxDistinct, Action<TPrimitive>? verify = null) : Spec<TPrimitive>
{
    private readonly Action<TPrimitive> _verify = verify ?? (p => {});
    [Fact] public void GivenSUT_ThenIsNotDefault() => When(_ => _).Then().Result.Is().Not(default(TPrimitive));
    [Fact] public void GivenSingle_ThenIsNotDefault() => A<TPrimitive>().Is().Not(default(TPrimitive));
    [Fact]
    public void GivenMaxDistinctValues_ThenAreDifferent()
        => Enumerable.Range(1, maxDistinct).Select(n => Any<TPrimitive>()).Is().Distinct().and.Has().All(_verify);
}

public class WhenGenerateInt() : WhenGeneratePrimitive<int>(100);
public class WhenGenerateUInt() : WhenGeneratePrimitive<uint>(100);
public class WhenGenerateByte() : WhenGeneratePrimitive<byte>(100);
public class WhenGenerateSByte() : WhenGeneratePrimitive<sbyte>(100);
public class WhenGenerateShort() : WhenGeneratePrimitive<short>(100);
public class WhenGenerateUShort() : WhenGeneratePrimitive<ushort>(100);
public class WhenGenerateLong() : WhenGeneratePrimitive<long>(100);
public class WhenGenerateULong() : WhenGeneratePrimitive<ulong>(100);

public class WhenGenerateFloat() : WhenGeneratePrimitive<float>(100, v => (v % 1).Is().Not(0));

public class WhenGenerateDouble() : WhenGeneratePrimitive<double>(100, v => (v % 1).Is().Not(0));

public class WhenGenerateDecimal() : WhenGeneratePrimitive<decimal>(100, v => (v % 1).Is().Not(0));

public class WhenGenerateBool() : WhenGeneratePrimitive<bool>(2);
public class WhenGenerateChar() : WhenGeneratePrimitive<char>(94, v => v.Is().GreaterThan((char)32).and.LessThan((char)127));

public class WhenGenerateDateTime() : WhenGeneratePrimitive<DateTime>(100, v => v.Is().After(new DateTime(1900, 1, 1)))
{
    [Fact]
    public void GivenSingle_ThenIsUtc()
            => A<DateTime>().Has(d => d.Kind == DateTimeKind.Utc);
}

public class WhenGenerateTimeSpan() : WhenGeneratePrimitive<TimeSpan>(100);
public class WhenGenerateDateOnly() : WhenGeneratePrimitive<DateOnly>(100, v => v.Is().After(new DateOnly(1900, 1, 1)));

public class WhenGenerateDateTimeOffset() : WhenGeneratePrimitive<DateTimeOffset>(
    100, v => v.Is().After(new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero)));

public class WhenGenerateTimeOnly() : WhenGeneratePrimitive<TimeOnly>(100);
public class WhenGenerateGuid() : WhenGeneratePrimitive<Guid>(100, v => v.Is().Not(Guid.Empty));

public class WhenGenerateString() : WhenGeneratePrimitive<string>(100, v => v.Is().not.Empty());

public class WhenGenerateUri() : WhenGeneratePrimitive<Uri>(100, v => v.IsAbsoluteUri.Is(true));