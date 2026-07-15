using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenApplyTaskOfImplicitlyCastPrimitiveToMock : Spec<MyValueIntService, object>
{
    public WhenApplyTaskOfImplicitlyCastPrimitiveToMock() 
        => When(_ => _.SetValueAsync(A<MyValueInt>()));

    [Fact]
    public void Then_ItIsApplied()
    {
        Then<IMyValueIntRepo>(_ => _.SetAsync(The<MyValueInt>()));
        Specification.Is(
@"When _.SetValueAsync(a MyValueInt)
Then IMyValueIntRepo.SetAsync(the MyValueInt)");
    }
}