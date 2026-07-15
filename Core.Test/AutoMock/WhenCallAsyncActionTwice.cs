namespace TSpec.Test.AutoMock;

public abstract class WhenCallAsyncActionTwice : Spec<InterfaceService>
{
    protected WhenCallAsyncActionTwice()
    {
        When(async _ =>
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
        public void ThenDoesNotThrow() => Then().DoesNotThrow();
    }
}