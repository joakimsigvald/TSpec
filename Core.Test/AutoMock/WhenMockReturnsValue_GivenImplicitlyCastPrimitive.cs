using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenMockReturnsValue_GivenImplicitlyCastPrimitive : Spec<MyValueIntService, string>
{
    private const string RetVal = "abc";

    public WhenMockReturnsValue_GivenImplicitlyCastPrimitive()
        => When(_ => _.GetValue(A<MyValueInt>()))
        .Given<IMyValueIntRepo>().That(_ => _.Get(The<MyValueInt>())).Returns(() => RetVal);

    [Fact]
    public void Then_ItReturnsExpectedValue()
    {
        Result.Is(RetVal);
        Specification.Is(
            """
            Given IMyValueIntRepo.Get(the MyValueInt) returns RetVal
            When _.GetValue(a MyValueInt)
            Then Result is RetVal
            """);
    }
}