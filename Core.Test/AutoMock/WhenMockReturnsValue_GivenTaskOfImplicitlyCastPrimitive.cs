using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenMockReturnsValue_GivenTaskOfImplicitlyCastPrimitive : Spec<MyValueIntService, string>
{
    private const string _retVal = "abc";

    public WhenMockReturnsValue_GivenTaskOfImplicitlyCastPrimitive()
        => When(_ => _.GetValueAsync(A<MyValueInt>()))
        .Given<IMyValueIntRepo>().That(_ => _.GetAsync(The<MyValueInt>())).Returns(() => _retVal);

    [Fact]
    public void Then_ItReturnsExpectedValue()
    {
        Result.Is(_retVal);
        Specification.Is(
            """
            Given IMyValueIntRepo.GetAsync(the MyValueInt) returns _retVal
            When _.GetValueAsync(a MyValueInt)
            Then Result is _retVal
            """);
    }
}