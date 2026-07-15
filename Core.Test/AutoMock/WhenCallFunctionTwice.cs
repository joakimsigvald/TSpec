using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public abstract class WhenCallFunctionTwice : Spec<InterfaceService, int>
{
    protected WhenCallFunctionTwice() => When(_ => TryGetValue(_) + _.GetServiceValue());


    private int TryGetValue(InterfaceService _)
    {
        try
        {
            return _.GetServiceValue();
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public class GivenOneMockedResponse : WhenCallFunctionTwice
    {
        public GivenOneMockedResponse() => Given<IMyService>().That(_ => _.GetValue()).Returns(() => 1);
        [Fact] public void ThenReturnThatResponsTwice() => Then().Result.Is(2);
    }

    public class GivenAndNextWithoutFirst : WhenCallFunctionTwice
    {
        [Fact]
        public void ThenThrowSetupFailed()
        {
            var ex = Xunit.Assert.Throws<SetupFailed>(
                () => Given<IMyService>().That(_ => _.GetValue()).Returns(() => 1).AndNext().Returns(() => 1)
                .Then().Result.Is(2));
            ex.Message.Does().Contain("First");
        }
    }

    public class GivenDifferentResponse : WhenCallFunctionTwice
    {
        public GivenDifferentResponse()
            => Given<IMyService>().That(_ => _.GetValue()).First().Returns(() => 1).AndNext().Returns(() => 2);
        [Fact] public void ThenReturnDifferentResponses() => Then().Result.Is(3);
    }

    public class GivenThrowsSecondTime : WhenCallFunctionTwice
    {
        public GivenThrowsSecondTime()
            => Given<IMyService>().That(_ => _.GetValue()).First().Returns(() => 1).AndNext().Throws(An<ArgumentException>);
        [Fact] public void ThenThrows() => Then().Throws(The<ArgumentException>);
    }

    public class GivenThrowsFirstTime : WhenCallFunctionTwice
    {
        public GivenThrowsFirstTime()
            => Given<IMyService>().That(_ => _.GetValue()).First().Throws<ArgumentException>().AndNext().Returns(An<int>);

        [Fact] public void ThenReturnsSecondValue() => Result.Is(The<int>());
    }
}