using TSpec.Assert;

namespace TSpec.Test.Pipeline;

public class WhenAsyncTestMethod : Spec<MyStateService, int>
{
    public WhenAsyncTestMethod() => When(_ => ++_.Counter);

    [Fact]
    public async Task ThenSpecificationIsRecordedAfterResumingOnAnotherThread()
    {
        // The specification context must survive await-resumption on a different thread
        var startThread = Environment.CurrentManagedThreadId;
        for (var i = 0; i < 100 && Environment.CurrentManagedThreadId == startThread; i++)
            await Task.Delay(1, TestContext.Current.CancellationToken);

        Then().Result.Is(1);
        Specification.Is(
            """
            When ++_.Counter
            Then Result is 1
            """);
    }
}
