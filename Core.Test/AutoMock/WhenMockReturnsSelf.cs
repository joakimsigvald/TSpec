using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenMockReturnsSelf : Spec<MyValueIntService, IMyValueIntRepo>
{
    public WhenMockReturnsSelf() => When(_ => _.GetRepo());

    [Fact]
    public void Then_ReturnIt()
    {
        Result.GetObject().GetType().Is(typeof(object));
        Specification.Is(
            """
            When _.GetRepo()
            Then Result.GetObject().GetType() is typeof object
            """);
    }
}