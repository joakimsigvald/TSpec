using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenSetupMultipleMethodsOnMock : Spec<MyValueIntService, string>
{
    [Fact]
    public void ThenSetupAll()
    {
        When(_ => _.GetValue(A<MyValueInt>()))
            .Given<IMyValueIntRepo>()
            .That(_ => _.GetAsync(The<MyValueInt>())).Returns(A<string>)
            .AndThat(_ => _.Get(The<MyValueInt>())).Returns(ASecond<string>)
            .Then().Result.Is(TheSecond<string>());
        Specification.Is(
            """
            Given IMyValueIntRepo.GetAsync(the MyValueInt) returns a string
              and Get(the MyValueInt) returns a second string
            When _.GetValue(a MyValueInt)
            Then Result is the second string
            """);
    }
}