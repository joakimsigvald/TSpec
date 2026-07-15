namespace TSpec.Internal.TestData;

internal interface IRepository
{
    bool TryGetDefault(Type type, For scope, out object? val);
    (object? val, bool found) Use(Type type, For scope);
    object Create(Type type, For scope);
}