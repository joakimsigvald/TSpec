namespace TSpec.Internal.Pipelines;

internal static class AsyncHelper
{
    public static void Execute(Func<Task> action)
        => _taskFactory.StartNew(action).Unwrap().GetAwaiter().GetResult();

    public static TResult Execute<TResult>(Func<Task<TResult>> func)
        => _taskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();

    private static readonly TaskFactory _taskFactory = new
        (CancellationToken.None,
        TaskCreationOptions.None,
        TaskContinuationOptions.None,
        TaskScheduler.Default);
}