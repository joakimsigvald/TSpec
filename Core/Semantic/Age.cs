namespace TSpec.Semantic;

/// <summary>
/// Represents an age below 120
/// </summary>
/// <param name="Value">The underlying integer representation of the age.</param>
public record Age(int Value) : Semantic<int>(Value), ISemantic<int>
{
    /// <summary>
    /// Generates an age below 120
    /// </summary>
    /// <param name="seed">A unique incremental number used as a seed.</param>
    /// <returns>A primitive integer representing the age.</returns>
    public static int GenerateValue(int seed) => seed % 120;
}