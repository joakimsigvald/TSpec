namespace TSpec.Internal;

/// <summary>
/// Deprecation messages, kept in one place so an obsolete member and the interface declaring it
/// cannot drift apart — the compiler requires a constant, so each site would otherwise repeat it.
/// </summary>
internal static class Obsoletions
{
    /// A setup on the whole type says where every value of that type comes from, which is what
    /// Using arranges; Given arranges the values and collaborators of this one test.
    internal const string TypeSetup =
        "A setup on the whole type is a type arrangement — use Using<TValue>(setup) instead. "
        + "To set up one particular value, use Given().A<TValue>(setup).";
}
