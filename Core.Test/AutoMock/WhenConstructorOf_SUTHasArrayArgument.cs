using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenConstructorOf_SUTTakeArrayOfObject: Spec<ArrayService, SomeValue[]>
{
    [Fact]
    public void ThenArrayIsEmpty()
    {
        When(_ => _.GetValues()).Then().Result.Is().Empty();
        Specification.Is(
@"When _.GetValues()
Then Result is empty");
    }
}

public class ArrayService(SomeValue[] values)
{
    private readonly SomeValue[] _values = values;
    public SomeValue[] GetValues() => _values;
}

public class SomeValue
{
}