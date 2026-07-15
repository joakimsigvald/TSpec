namespace TSpec.Semantic;

/// <summary>
/// Represents a syntactically valid email address.
/// Can be used as a strongly-typed replacement for raw strings in test data generation.
/// </summary>
/// <param name="Value">The underlying string representation of the email address.</param>
public record Email(string Value) : Semantic<string>(Value), ISemantic<string>
{
    private static readonly string[] _tlds = [".com", ".org", ".net", ".io", ".co.uk", ".se"];

    /// <summary>
    /// Generates a deterministic email address based on the provided counter.
    /// </summary>
    /// <param name="seed">A unique incremental number used as a seed.</param>
    /// <returns>A primitive string representing the email address.</returns>
    public static string GenerateValue(int seed)
    {
        var tld = _tlds[seed % _tlds.Length];

        // Output example: user1@example.org, user2@example.net, etc.
        return $"user{seed}@example{tld}";
    }
}