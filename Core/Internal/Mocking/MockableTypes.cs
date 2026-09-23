namespace TSpec.Internal.Mocking;

internal static class MockableTypes
{
    /// A class the test sets up is mocked as well, so every type but a sealed class or a value can be.
    internal static bool IsMockable(Type type)
        => IsMockedByDefault(type) || type is { IsClass: true, IsSealed: false };

    internal static bool IsMockedByDefault(Type type)
        => type.IsInterface
        || type.IsAbstract
        || typeof(Delegate).IsAssignableFrom(type);
}
