namespace TSpec.Semantic;

/// <summary>
/// Represents an E.164-style placeholder international phone number (not guaranteed to be a real, dialable number).
/// Can be used as a strongly-typed replacement for raw strings in test data generation.
/// </summary>
/// <param name="Value">The underlying string representation of the phone number.</param>
public record PhoneNumber(string Value) : Semantic<string>(Value), ISemantic<string>
{
    private static readonly string[] _countryCodes = ["1", "44", "46", "81"];

    /// <summary>
    /// Generates a deterministic phone number based on the provided counter.
    /// </summary>
    /// <param name="seed">A unique incremental number used as a seed.</param>
    /// <returns>A primitive string representing the phone number.</returns>
    public static string GenerateValue(int seed)
    {
        var cc = _countryCodes[seed % _countryCodes.Length];
        return $"+{cc}555{seed:D6}";
    }
}