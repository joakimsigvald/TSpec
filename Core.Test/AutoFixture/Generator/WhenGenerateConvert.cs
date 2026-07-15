using TSpec.Assert;
using TSpec.Internal.TestData.Generation;
using TSpec.Semantic;
using TSpec.Test.AutoFixture.Primitives;

namespace TSpec.Test.AutoFixture.Generator;

public abstract class WhenGenerateDerivedValueType<TValueType>(int maxDistinct) : Spec<TValueType>
{
    [Fact]
    public void GivenMaxDistinctValues_ThenAreDifferent()
        => Enumerable.Range(1, maxDistinct).Select(n => Any<TValueType>()).Is().Distinct();
}

public class WhenConvertByConstructor : WhenGenerateDerivedValueType<MyEmailConstr>
{
    public WhenConvertByConstructor() : base(100) => Using<MyEmailConstr>().From<Email>();
    [Fact] public void ThenHasValidEmailFormat() => An<MyEmailConstr>().Value.Has(ValidEmail.Verify);
}

public class WhenConvertByStaticCreate : WhenGenerateDerivedValueType<MyEmailConstr>
{
    public WhenConvertByStaticCreate() : base(100) => Using<MyEmailStatic>().From<Email>();
    [Fact] public void ThenHasValidEmailFormat() => An<MyEmailStatic>().Value.Has(ValidEmail.Verify);
}

public class WhenConvertByExplicitCast : WhenGenerateDerivedValueType<MyEmailConstr>
{
    public WhenConvertByExplicitCast() : base(100) => Using<MyEmailCast>().From<Email>();
    [Fact] public void ThenHasValidEmailFormat() => An<MyEmailCast>().Value.Has(ValidEmail.Verify);
}

public class WhenRelayIntToByte : Spec<int>
{
    public WhenRelayIntToByte() => Using<int>().From<byte>();
    [Fact] public void ThenGenerateByteAsInt() => Three<int>().Is().EqualTo([1, 2, 3]);
}

public class WhenRelayIntToByteWithConverter : Spec<int>
{
    public WhenRelayIntToByteWithConverter() => Using<int>().From((byte b) => b + 1);
    [Fact] public void ThenGenerateByteAsInt() => Three<int>().Is().EqualTo([2, 3, 4]);
}

public class WhenRelayByteToInt : Spec<byte>
{
    public WhenRelayByteToInt() => Using<byte>().From<int>();
    [Fact] public void ThenGenerateIntAsByte() => Three<byte>().Is().EqualTo([1, 2, 3]);
}

public class WhenRelayStringToByte : Spec<string>
{
    public WhenRelayStringToByte() => Using<string>().From<byte>();
    [Fact] public void ThenGenerateByteAsString() => Three<string>().Is().EqualTo(["1", "2", "3"]);
}

public class WhenRelayIncompatibleTypes : Spec<byte>
{
    public WhenRelayIncompatibleTypes() => Using<byte>().From<DateTime>();

    [Fact]
    public void ThenThrowsInvalidTypeConversion()
        => Xunit.Assert.Throws<InvalidTypeConversion>(() => A<byte>().Is(0));
}

public record MyEmailConstr(string Value);
public record MyEmailStatic
{
    public string Value { get; private set; } = null!;
    public static MyEmailStatic Make(string value) => new() { Value = value };
}

public record MyEmailCast
{
    public string Value { get; private set; } = null!;
    public static implicit operator MyEmailCast(string value) => new() { Value = value };
}