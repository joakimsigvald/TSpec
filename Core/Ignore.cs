namespace TSpec;

/// <summary>
/// This type's sole purpose is to help the compiler distinguish between overloaded extension methods,
/// by adding it as nullable to the argument list. As the name implies, this type should be ignored.
/// </summary>
public readonly struct Ignore
{
    /// <summary>
    /// The Singleton instance of Ignore
    /// </summary>
    public readonly static Ignore Me = default;

    /// <summary>
    /// Represent this instance as text
    /// </summary>
    /// <returns>The empty string</returns>
    public override string ToString() => string.Empty;
}