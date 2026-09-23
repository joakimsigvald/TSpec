using TSpec.Internal.Specification;
using TSpec.Internal.TestData;
using TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

namespace TSpec.Internal.Mocking;

internal class FluentDefaultProvider(IRepository repository)
{
    private readonly Dictionary<Type, Func<Exception>> _defaultExceptions = [];
    private readonly Dictionary<Type, Dictionary<Type, object?>> _providedDefaults = [];

    internal object? GetDefaultValue(Type type, MockHandle mock, CallAddress address)
    {
        if (TryGetProvidedDefault(type, mock, out var provided))
            return provided!;
        if (TryGetProvidedAsyncResult(type, mock, out var providedAsync))
            return providedAsync!;
        var ex = GetDefaultException(mock.MockedType);
        if (ex is not null)
            throw ex;
        var (val, found) = repository.Use(type, For.Subject);
        return found ? val!
            : IsReturningSelf(type, mock) ? mock.Instance
            : IsTask(type) ? GetTask(type, mock, address)
            : IsValueTask(type) ? GetValueTask(type, mock, address)
            : mock.PerAddress(type, repository.Create(type, For.Subject), address);
    }

    internal object?[] GetConstructorArguments(Type mockedType) => repository.CreateMockConstructorArguments(mockedType);

    private Exception? GetDefaultException(Type type)
        => _defaultExceptions.TryGetValue(type, out var ex) ? ex() : null;

    internal void SetDefaultException(Type type, Func<Exception> ex)
        => _defaultExceptions[type] = ex;

    internal bool IsSetUp(Type service)
        => _providedDefaults.ContainsKey(service) || _defaultExceptions.ContainsKey(service);

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

    private bool TryGetProvidedAsyncResult(Type type, MockHandle mock, out object? result)
    {
        result = null;
        if (!IsTask(type) && !IsValueTask(type) || type.GenericTypeArguments is not [var valueType])
            return false;
        if (!TryGetProvidedDefault(valueType, mock, out var provided))
            return false;
        result = IsTask(type)
            ? TaskCompiler.GetFromResultMethod(valueType)(provided!)
            : ValueTaskCompiler.GetFromResultMethod(valueType)(provided!);
        return true;
    }

    private static Type MostSpecific(Type[] candidates, Type returnType, Type service)
        => candidates.FirstOrDefault(candidate => candidates.All(candidate.IsAssignableTo))
        ?? throw new SetupFailed(
            @$"{service.Alias()} returns {returnType.Alias()}, and no provided default is more specific than the others: {
                string.Join(", ", candidates.Select(_ => _.Alias()))}.
Provide a value for {returnType.Alias()} itself to say which one applies.");

    private static bool IsReturningSelf(Type type, MockHandle mock)
        => !type.IsAssignableFrom(typeof(object)) && type.IsAssignableFrom(mock.Instance.GetType());

    private static bool IsTask(Type type) => typeof(Task).IsAssignableFrom(type);

    private static bool IsValueTask(Type type)
        => type == typeof(ValueTask)
        || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>);

    private Task GetTask(Type type, MockHandle mock, CallAddress address)
        => type == typeof(Task) ? Task.CompletedTask : GetTaskOf(type.GenericTypeArguments.Single(), mock, address);

    private Task GetTaskOf(Type valueType, MockHandle mock, CallAddress address)
        => TaskCompiler.GetFromResultMethod(valueType)(GetDefaultValue(valueType, mock, address)!);

    private object GetValueTask(Type type, MockHandle mock, CallAddress address)
        => type == typeof(ValueTask) ? default(ValueTask) : GetValueTaskOf(type.GenericTypeArguments.Single(), mock, address);

    private object GetValueTaskOf(Type valueType, MockHandle mock, CallAddress address)
        => ValueTaskCompiler.GetFromResultMethod(valueType)(GetDefaultValue(valueType, mock, address)!);
}