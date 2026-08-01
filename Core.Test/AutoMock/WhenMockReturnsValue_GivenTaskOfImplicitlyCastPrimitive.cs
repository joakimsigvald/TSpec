using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenMockReturnsValue_GivenTaskOfImplicitlyCastPrimitive : Spec<MyValueIntService, string>
{
    private const string RetVal = "abc";

    public WhenMockReturnsValue_GivenTaskOfImplicitlyCastPrimitive()
        => When(_ => _.GetValueAsync(A<MyValueInt>()))
        .Given<IMyValueIntRepo>().That(_ => _.GetAsync(The<MyValueInt>())).Returns(() => RetVal);

    [Fact]
    public void Then_ItReturnsExpectedValue()
    {
        Result.Is(RetVal);
        Specification.Is(
            """
            Given IMyValueIntRepo.GetAsync(the MyValueInt) returns RetVal
            When GetValueAsync(a MyValueInt)
            Then Result is RetVal
            """);
    }
}