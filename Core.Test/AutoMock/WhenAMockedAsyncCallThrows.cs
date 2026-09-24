using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public interface IAsyncStore
{
    Task<string> GetAsync(int id);
    Task SaveAsync(int id);
    ValueTask<int> CountAsync(int id);
    ValueTask TouchAsync(int id);
    int Count(int id);
}

public abstract class AsyncChannel
{
    protected abstract Task FlushAsync(int id);
    public Task Flush(int id) => FlushAsync(id);
}

/// Reports how a call failed: thrown at the caller, or handed back in a faulted task.
public class AsyncCaller(IAsyncStore store, AsyncChannel channel)
{
    public string Get() => HowItFails(() => store.GetAsync(1));
    public string Save() => HowItFails(() => store.SaveAsync(1));
    public string CountAsync() => HowItFails(() => store.CountAsync(1).AsTask());
    public string Touch() => HowItFails(() => store.TouchAsync(1).AsTask());
    public string Flush() => HowItFails(() => channel.Flush(1));

    public string Count()
    {
        try
        {
            return $"answered {store.Count(1)}";
        }
        catch (Exception ex)
        {
            return $"thrown {ex.GetType().Name}";
        }
    }

    private static string HowItFails(Func<Task> call)
    {
        Task task;
        try
        {
            task = call();
        }
        catch (Exception ex)
        {
            return $"thrown {ex.GetType().Name}";
        }
        try
        {
            task.GetAwaiter().GetResult();
            return "completed";
        }
        catch (Exception ex)
        {
            return $"faulted {ex.GetType().Name}";
        }
    }
}

/// <summary>
/// A real async method never throws at its caller: what it throws comes back in the task. A mocked
/// one does the same, however the throw was set up — so a caller that starts several calls before
/// awaiting any of them sees every one of them start.
/// </summary>
public class WhenAMockedAsyncCallThrows : Spec<AsyncCaller, string>
{
    [Fact]
    public void GivenThrowsByType_ThenTheTaskFaultsWithIt()
        => When(_ => _.Get())
            .Given<IAsyncStore>().That(_ => _.GetAsync(1)).Throws<ArgumentException>()
            .Then().Result.Is("faulted ArgumentException");

    [Fact]
    public void GivenReturnsThatThrows_ThenTheTaskFaults()
        => When(_ => _.Get())
            .Given<IAsyncStore>().That(_ => _.GetAsync(1)).Returns(() => throw new ArgumentException())
            .Then().Result.Is("faulted ArgumentException");

    [Fact]
    public void GivenASequenceStepThrows_ThenTheTaskFaults()
        => When(_ => _.Get())
            .Given<IAsyncStore>().That(_ => _.GetAsync(1)).First().Throws<ArgumentException>()
            .Then().Result.Is("faulted ArgumentException");

    [Fact]
    public void GivenATaskWithoutValueThrows_ThenTheTaskFaults()
        => When(_ => _.Save())
            .Given<IAsyncStore>().That(_ => _.SaveAsync(1)).Throws<ArgumentException>()
            .Then().Result.Is("faulted ArgumentException");

    [Fact]
    public void GivenAValueTaskThrows_ThenTheTaskFaults()
        => When(_ => _.CountAsync())
            .Given<IAsyncStore>().That(_ => _.CountAsync(1)).Returns(() => throw new ArgumentException())
            .Then().Result.Is("faulted ArgumentException");

    [Fact]
    public void GivenAValueTaskWithoutValueThrows_ThenTheTaskFaults()
        => When(_ => _.Touch())
            .Given<IAsyncStore>().That(_ => _.TouchAsync(1)).Throws<ArgumentException>()
            .Then().Result.Is("faulted ArgumentException");

    [Fact]
    public void GivenAProtectedTaskThrows_ThenTheTaskFaults()
        => When(_ => _.Flush())
            .Given<AsyncChannel>().That("FlushAsync").Throws<ArgumentException>()
            .Then().Result.Is("faulted ArgumentException");

    [Fact]
    public void GivenASynchronousCallThrows_ThenItThrows()
        => When(_ => _.Count())
            .Given<IAsyncStore>().That(_ => _.Count(1)).Throws<ArgumentException>()
            .Then().Result.Is("thrown ArgumentException");

    [Fact]
    public void GivenATaskReturnsDefault_ThenTheTaskCompletes()
        => When(_ => _.Save())
            .Given<IAsyncStore>().That(_ => _.SaveAsync(1)).ReturnsDefault()
            .Then().Result.Is("completed");
}
