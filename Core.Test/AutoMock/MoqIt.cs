namespace Moq;

/// <summary>
/// Stands in for Moq's matcher, which a test project that still references Moq could write inside
/// a TSpec setup. TSpec recognises it by name and refuses it.
/// </summary>
public static class It
{
    public static T IsAny<T>() => default!;
}
