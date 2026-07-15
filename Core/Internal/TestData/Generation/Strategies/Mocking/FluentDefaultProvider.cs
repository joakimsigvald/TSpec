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
            : repository.Create(type, For.Subject);
    }

    private Exception? GetDefaultException(Type type)
        => _defaultExceptions.TryGetValue(type, out var ex) ? ex() : null;

    internal void SetDefaultException(Type type, Func<Exception> ex)
        => _defaultExceptions[type] = ex;

    private static bool IsReturningSelf(Type type, Mock mock)
        => !type.IsAssignableFrom(typeof(object)) && type.IsAssignableFrom(mock.Object.GetType());

    private static bool IsTask(Type type) => typeof(Task).IsAssignableFrom(type);

    private Task GetTask(Type type, Mock mock)
        => type == typeof(Task)
        ? Task.CompletedTask
        : GetTaskOf(type.GenericTypeArguments.Single(), mock);

    private Task GetTaskOf(Type valueType, Mock mock)
    {
        var value = GetDefaultValue(valueType, mock);
        if (value.GetType() != valueType)
        {
            var mockName = MockCompiler.GetMockedType(mock).Name;
            throw new SetupFailed(
                @$"{mockName} returns a Task<{valueType.Name}>. 
Interface types returned as task must be provided explicitly in the test setup.
You can provide a default interface instance with 'Given<{mockName}>().Returns(A<{valueType.Name}>)'.");
        }
        return TaskCompiler.GetFromResultMethod(valueType)(value);
    }
}