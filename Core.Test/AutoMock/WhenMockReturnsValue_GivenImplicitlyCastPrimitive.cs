using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenMockReturnsValue_GivenImplicitlyCastPrimitive : Spec<MyValueIntService, string>
{
    private const string _retVal = "abc";

    public WhenMockReturnsValue_GivenImplicitlyCastPrimitive()
        => When(_ => _.GetValue(A<MyValueInt>()))
        .Given<IMyValueIntRepo>().That(_ => _.Get(The<MyValueInt>())).Returns(() => _retVal);

    [Fact]
    public void Then_ItReturnsExpectedValue()
    {
        Result.Is(_retVal);
        Specification.Is(
            """
            Given IMyValueIntRepo.Get(the MyValueInt) returns _retVal
            When _.GetValue(a MyValueInt)
            Then Result is _retVal
            """);
    }
}