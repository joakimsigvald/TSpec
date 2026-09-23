using System.Collections.Concurrent;
using System.Reflection;
using TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

namespace TSpec.Internal.Mocking;

/// <summary>
/// How an answer reaches a call that is awaited. A real async method never throws at its caller, so
/// neither does a mocked one: whatever the answer throws comes back as a faulted task.
/// </summary>
internal static class AsyncAnswer
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _faulted = [];

    /// <summary>
    /// The call's result, given what the answer produced. An answer of the call's own type is handed
    /// back as it is; any other is the value inside the task, or nothing where the task holds none.
    /// </summary>
    internal static object? Respond(Type returnType, Type answerType, Func<object?> answer)
    {
        if (!IsAsync(returnType))
            return answer();
        try
        {
            var value = answer();
            return returnType == answerType ? value : Completed(returnType, value);
        }
        catch (Exception ex)
        {
            return Faulted(returnType, ex);
        }
    }

    /// What a task that has already completed holds, without waiting for one that has not; any other
    /// answer is its own value.
    internal static object? ValueOf(object? answer)
    {
        if (answer is null || !IsAsyncOfValue(answer.GetType()))
            return answer;

        var type = answer.GetType();
        return type.GetProperty(nameof(Task.IsCompletedSuccessfully))!.GetValue(answer) is true
            ? type.GetProperty(nameof(Task<object>.Result))!.GetValue(answer)
            : null;
    }

    internal static bool IsAsyncOfValue(Type type)
        => type.IsGenericType && IsAsyncOfValueDefinition(type.GetGenericTypeDefinition());

    private static bool IsAsync(Type type)
        => type == typeof(Task)
        || type == typeof(ValueTask)
        || IsAsyncOfValue(type);

    private static bool IsAsyncOfValueDefinition(Type definition)
        => definition == typeof(Task<>) || definition == typeof(ValueTask<>);

    private static object Completed(Type type, object? value)
        => type == typeof(Task) ? Task.CompletedTask
        : type == typeof(ValueTask) ? default(ValueTask)
        : type.GetGenericTypeDefinition() == typeof(Task<>)
            ? TaskCompiler.GetFromResultMethod(type.GenericTypeArguments[0])(value!)
        : ValueTaskCompiler.GetFromResultMethod(type.GenericTypeArguments[0])(value!);

    private static object Faulted(Type type, Exception ex)
        => type == typeof(Task) ? Task.FromException(ex)
        : type == typeof(ValueTask) ? new ValueTask(Task.FromException(ex))
        : _faulted.GetOrAdd(type, FaultedOf).Invoke(null, [ex])!;

    private static MethodInfo FaultedOf(Type type)
        => typeof(AsyncAnswer)
            .GetMethod(
                type.GetGenericTypeDefinition() == typeof(Task<>) ? nameof(FaultedTask) : nameof(FaultedValueTask),
                BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(type.GenericTypeArguments[0]);

    private static Task<TValue> FaultedTask<TValue>(Exception ex) => Task.FromException<TValue>(ex);

    private static object FaultedValueTask<TValue>(Exception ex) => new ValueTask<TValue>(Task.FromException<TValue>(ex));
}
