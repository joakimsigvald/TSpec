using TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

internal class FluentDefaultProvider(IRepository repository)
{
    private readonly Dictionary<Type, Func<Exception>> _defaultExceptions = [];
    private readonly Dictionary<Type, Dictionary<Type, object?>> _providedDefaults = [];

    internal object GetDefaultValue(Type type, MockHandle mock)
    {
        var ex = GetDefaultException(mock.MockedType);
        if (ex is not null)
            throw ex;
        var (val, found) = repository.Use(type, For.Subject);
        return found ? val!
            : IsReturningSelf(type, mock) ? mock.Instance
            : IsTask(type) ? GetTask(type, mock)
            : IsValueTask(type) ? GetValueTask(type, mock)
            : TryGetProvidedDefault(type, mock, out var provided) ? provided!
            : repository.Create(type, For.Subject);
    }

    private Exception? GetDefaultException(Type type)
        => _defaultExceptions.TryGetValue(type, out var ex) ? ex() : null;

    internal void SetDefaultException(Type type, Func<Exception> ex)
        => _defaultExceptions[type] = ex;

    internal void SetProvidedDefault(Type service, Type providedType, object? value)
        => GetProvidedDefaults(service)[providedType] = value;

    private Dictionary<Type, object?> GetProvidedDefaults(Type service)
        => _providedDefaults.TryGetValue(service, out var provided) ? provided : _providedDefaults[service] = [];

    private bool TryGetProvidedDefault(Type type, MockHandle mock, out object? value)
    {
        value = null;
        var service = mock.MockedType;
        if (!_providedDefaults.TryGetValue(service, out var provided))
            return false;
        var candidates = provided.Keys.Where(type.IsAssignableFrom).ToArray();
        if (candidates.Length == 0)
            return false;
        value = provided[candidates.Length == 1 ? candidates[0] : MostSpecific(candidates, type, service)];
        return true;
    }

    private static Type MostSpecific(Type[] candidates, Type returnType, Type service)
        => candidates.FirstOrDefault(candidate => candidates.All(candidate.IsAssignableTo))
        ?? throw new SetupFailed(
            @$"{service.Name} returns {returnType.Name}, and no provided default is more specific than the others: {
                string.Join(", ", candidates.Select(_ => _.Name))}.
Provide a value for {returnType.Name} itself to say which one applies.");

    private static bool IsReturningSelf(Type type, MockHandle mock)
        => !type.IsAssignableFrom(typeof(object)) && type.IsAssignableFrom(mock.Instance.GetType());

    private static bool IsTask(Type type) => typeof(Task).IsAssignableFrom(type);

    private static bool IsValueTask(Type type)
        => type == typeof(ValueTask)
        || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>);

    private Task GetTask(Type type, MockHandle mock)
        => type == typeof(Task) ? Task.CompletedTask : GetTaskOf(type.GenericTypeArguments.Single(), mock);

    private Task GetTaskOf(Type valueType, MockHandle mock)
        => TaskCompiler.GetFromResultMethod(valueType)(GetAsyncResult(valueType, mock, nameof(Task)));

    private object GetValueTask(Type type, MockHandle mock)
        => type == typeof(ValueTask) ? default(ValueTask) : GetValueTaskOf(type.GenericTypeArguments.Single(), mock);

    private object GetValueTaskOf(Type valueType, MockHandle mock)
        => ValueTaskCompiler.GetFromResultMethod(valueType)(GetAsyncResult(valueType, mock, nameof(ValueTask)));

    private object GetAsyncResult(Type valueType, MockHandle mock, string asyncType)
    {
        var value = GetDefaultValue(valueType, mock);
        if (value is null || value.GetType() == valueType)
            return value!;
        var mockName = mock.MockedType.Name;
        throw new SetupFailed(
            @$"{mockName} returns a {asyncType}<{valueType.Name}>.
Interface types returned as task must be provided explicitly in the test setup.
You can provide a default interface instance with 'Given<{mockName}>().Returns(A<{valueType.Name}>)'.");
    }
}