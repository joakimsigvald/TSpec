using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public abstract class WhenCallAsyncFunctionTwice : Spec<InterfaceService, int>
{
    protected WhenCallAsyncFunctionTwice()
        => When(async _ => await TryGetValue(_) + await _.GetServiceValueAsync());

    private async Task<int> TryGetValue(InterfaceService _)
    {
        try
        {
            return await _.GetServiceValueAsync();
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public class GivenOneMockedResponse : WhenCallAsyncFunctionTwice
    {
        public GivenOneMockedResponse() => Given<IMyService>().That(_ => _.GetValueAsync()).Returns(() => 1);
        [Fact]
        public void ThenReturnThatResponsTwice()
        {
            Then().Result.Is(2);
            Specification.Is(
                """
                Given IMyService.GetValueAsync() returns 1
                When async _ => await TryGetValue(_) + await _.GetServiceValueAsync()
                Then Result is 2
                """);
        }
    }

    public class GivenDifferentResponse : WhenCallAsyncFunctionTwice
    {
        public GivenDifferentResponse()
            => Given<IMyService>().That(_ => _.GetValueAsync()).First().Returns(() => 1).AndNext().Returns(() => 2);

        [Fact]
        public void ThenReturnDifferentResponses()
        {
            Then().Result.Is(3);
            Specification.Is(
                """
                Given IMyService.GetValueAsync() first returns 1
                  and next returns 2
                When async _ => await TryGetValue(_) + await _.GetServiceValueAsync()
                Then Result is 3
                """);
        }
    }

    public class GivenThrowsSecondTime : WhenCallAsyncFunctionTwice
    {
        public GivenThrowsSecondTime()
            => Given<IMyService>().That(_ => _.GetValueAsync())
            .First().Returns(() => 1)
            .AndNext().Throws(An<ArgumentException>)
            .AndNext().Returns();

        [Fact]
        public void ThenThrows()
        {
            Then().Throws(The<ArgumentException>);
            Specification.Is(
                """
                Given IMyService.GetValueAsync() first returns 1
                  and next throws an ArgumentException
                  and next returns
                When async _ => await TryGetValue(_) + await _.GetServiceValueAsync()
                Then throws the ArgumentException
                """);
        }
    }

    public class GivenThrowsFirstTime : WhenCallAsyncFunctionTwice
    {
        public GivenThrowsFirstTime()
            => Given<IMyService>().That(_ => _.GetValueAsync()).First().Throws(An<ArgumentException>).AndNext().Returns(An<int>);

        [Fact]
        public void ThenReturnsSecondValue()
        {
            Result.Is(The<int>());
            Specification.Is(
                """
                Given IMyService.GetValueAsync() first throws an ArgumentException
                  and next returns an int
                When async _ => await TryGetValue(_) + await _.GetServiceValueAsync()
                Then Result is the int
                """);
        }
    }
}