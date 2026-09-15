namespace TSpec.Test.AutoMock;

public abstract class WhenCallAsyncActionTwice : Spec<InterfaceService>
{
    protected WhenCallAsyncActionTwice()
    {
        When(async Task (_) =>
        {
            var task1 = TrySetValue(_, 1);
            await task1;
            var task2 = _.SetValueAsync(1);
            await task2;
        });
    }

    private async Task TrySetValue(InterfaceService _, int value)
    {
        try
        {
            await _.SetValueAsync(value);
        }
        catch (Exception)
        {
            return;
        }
    }

    public class GivenThrowsFirstTime : WhenCallAsyncActionTwice
    {
        public GivenThrowsFirstTime()
            => Given<IMyService>().That(_ => _.SetValueAsync(1))
            .First().Throws(An<ArgumentException>)
            .AndNext().Returns();

        [Fact]
        public void ThenCompletes() => Then().Completes();
    }

    /// Past its last step a sequence answers as ReturnsDefault does: a task that has completed.
    public class GivenCalledPastTheLastStep : WhenCallAsyncActionTwice
    {
        public GivenCalledPastTheLastStep()
            => Given<IMyService>().That(_ => _.SetValueAsync(1))
            .First().Returns();

        [Fact]
        public void ThenCompletes() => Then().Completes();
    }
}