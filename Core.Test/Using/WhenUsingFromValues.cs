using TSpec.Assert;
using TSpec.Internal.TestData.Generation;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenFromValues : Spec<int>
{
    public WhenFromValues() => Using<int>().From([10, 20, 30]);

    [Fact]
    public void ThenUseValuesInDeclarationOrder() => Three<int>().Is().EqualTo([10, 20, 30]);
}

public class WhenFromValuesWithDuplicates : Spec<int>
{
    public WhenFromValuesWithDuplicates() => Using<int>().From([7, 7, 8]);

    [Fact]
    public void ThenUseValuesAsProvided() => Three<int>().Is().EqualTo([7, 7, 8]);
}

public class WhenFromValuesExhausted : Spec<int>
{
    public WhenFromValuesExhausted() => Using<int>().From([10, 20]);

    [Fact]
    public void ThenYieldAllValuesBeforeThrowing()
    {
        Any<int>().Is(10);
        Any<int>().Is(20);
        Xunit.Assert.Throws<ValuesExhausted>(() => Any<int>());
    }
}

public class WhenFromValuesWithConversion : Spec<string>
{
    public WhenFromValuesWithConversion() => Using<string>().From([1, 2, 3]);

    [Fact]
    public void ThenConvertValues() => Three<string>().Is().EqualTo(["1", "2", "3"]);
}

public class WhenFromEmptyValues : Spec<int>
{
    [Fact]
    public void ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() => Using<int>().From<int>([]));
}

public class WhenFromValuesSpecification : Spec<MyService, int>
{
    public WhenFromValuesSpecification() => When(_ => _.GetNextId());

    [Fact]
    public void ThenMentionValues()
    {
        Using<int>(For.Subject).From([42, 43]).Then().Result.Is(42);
        Specification.Is(
            """
            Using int from [42, 43] for Subject
            When _.GetNextId()
            Then Result is 42
            """
            );
    }
}
