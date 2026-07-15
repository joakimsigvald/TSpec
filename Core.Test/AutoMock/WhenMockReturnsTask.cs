using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenMockReturnsTask : Spec<MyValueIntService, string>
{
    public WhenMockReturnsTask() => When(_ => _.SetAndGetValueAsync(An<int>()));

    [Fact]
    public void Then_CallMockReturnTask()
        => Then<IMyValueIntRepo>(_ => _.SetAsync(The<int>()))
            .And<IMyValueIntRepo>(_ => _.GetAsync(The<int>()));

    [Fact]
    public void GivenAssertionChainedAfterVerify_ThenDescribeResultWithoutBindingWord()
    {
        Then<IMyValueIntRepo>(_ => _.SetAsync(The<int>())).and.Result.Is().Not(null!);
        Specification.Is(
            """
            When _.SetAndGetValueAsync(an int)
            Then IMyValueIntRepo.SetAsync(the int)
              and Result is not null
            """);
    }
}