using Moq;
using TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class FluentDefaultProvider(IRepository repository) : DefaultValueProvider
{
    private readonly Dictionary<Type, Func<Exception>> _defaultExceptions = [];

    protected override object GetDefaultValue(Type type, Mock mock)
    {
        var ex = GetDefaultException(MockCompiler.GetMockedType(mock));
        if (ex is not null)
            throw ex;
        var (val, found) = repository.Use(type, For.Subject);
        return found ? val!
            : IsReturningSelf(type, mock) ? mock.Object
            : IsTask(type) ? GetTask(type, mock)
            : IsValueTask(type) ? GetValueTask(type, mock)
            : repository.Create(type, For.Subject);
    }

    private Exception? GetDefaultException(Type type)
        => _defaultExceptions.TryGetValue(type, out var ex) ? ex() : null;

    internal void SetDefaultException(Type type, Func<Exception> ex)
        => _defaultExceptions[type] = ex;

    private static bool IsReturningSelf(Type type, Mock mock)
        => !type.IsAssignableFrom(typeof(object)) && type.IsAssignableFrom(mock.Object.GetType());

    private static bool IsTask(Type type) => typeof(Task).IsAssignableFrom(type);

    private static bool IsValueTask(Type type)
        => type == typeof(ValueTask)
        || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>);

    private Task GetTask(Type type, Mock mock)
        => type == typeof(Task) ? Task.CompletedTask : GetTaskOf(type.GenericTypeArguments.Single(), mock);

    private Task GetTaskOf(Type valueType, Mock mock)
        => TaskCompiler.GetFromResultMethod(valueType)(GetAsyncResult(valueType, mock, nameof(Task)));

    private object GetValueTask(Type type, Mock mock)
        => type == typeof(ValueTask) ? default(ValueTask) : GetValueTaskOf(type.GenericTypeArguments.Single(), mock);

    private object GetValueTaskOf(Type valueType, Mock mock)
        => ValueTaskCompiler.GetFromResultMethod(valueType)(GetAsyncResult(valueType, mock, nameof(ValueTask)));

    private object GetAsyncResult(Type valueType, Mock mock, string asyncType)
    {
        var value = GetDefaultValue(valueType, mock);
        if (value.GetType() == valueType)
            return value;
        var mockName = MockCompiler.GetMockedType(mock).Name;
        throw new SetupFailed(
            @$"{mockName} returns a {asyncType}<{valueType.Name}>.
Interface types returned as task must be provided explicitly in the test setup.
You can provide a default interface instance with 'Given<{mockName}>().Returns(A<{valueType.Name}>)'.");
    }
}