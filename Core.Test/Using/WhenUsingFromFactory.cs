using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenFromFactory : Spec<int>
{
    private int _current = 1;
    private int _next = 1;

    public WhenFromFactory() => Using<int>().From(NextFibonacci);

    private int NextFibonacci()
    {
        var value = _current;
        (_current, _next) = (_next, _current + _next);
        return value;
    }

    [Fact]
    public void ThenGenerateValueSpaceWithDuplicates() => Four<int>().Is().EqualTo([1, 1, 2, 3]);
}

public class WhenFromFactoryWithConversion : Spec<string>
{
    private int _n = 40;

    public WhenFromFactoryWithConversion() => Using<string>().From(() => _n += 2);

    [Fact]
    public void ThenConvertGeneratedValues() => Three<string>().Is().EqualTo(["42", "44", "46"]);
}

public class WhenFactoryRepeatsValue : Spec<int>
{
    public WhenFactoryRepeatsValue() => Using<int>().From(() => 7);

    [Fact]
    public void ThenUseValuesAsProduced() => Three<int>().Is().EqualTo([7, 7, 7]);
}

public class WhenFromFactorySpecification : Spec<MyService, int>
{
    public WhenFromFactorySpecification() => When(_ => _.GetNextId());

    [Fact]
    public void ThenMentionFactory()
    {
        Using<int>(For.Subject).From(() => 42).Then().Result.Is(42);
        Specification.Is(
            """
            Using int from () => 42 for Subject
            When _.GetNextId()
            Then Result is 42
            """
            );
    }
}
