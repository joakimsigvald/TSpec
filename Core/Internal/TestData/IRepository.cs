namespace TSpec.Internal.TestData;

internal interface IRepository
{
    /// Resolves, rather than merely looks up: falls back to generating and mutating a value
    /// when no arrangement exists but a default setup is registered for the type.
    bool TryResolveDefault(Type type, For scope, out object? val);
    (object? val, bool found) Use(Type type, For scope);
    object Create(Type type, For scope);
}