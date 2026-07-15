namespace TSpec.Semantic;

/// <summary>
/// A lightweight wrapper for primitive types, used to create strongly-typed semantic categories 
/// (e.g., <c>Email</c> for <c>string</c>, or <c>Age</c> for <c>int</c>). 
/// Inherit from this record to define custom semantic types that can be seamlessly used in tests.
/// </summary>
/// <typeparam name="TPrimitive">The underlying primitive data type.</typeparam>
/// <param name="Value">The raw, underlying primitive value.</param>
public abstract record Semantic<TPrimitive>(TPrimitive Value)
{
    /// <summary>
    /// Implicitly unwraps the category into its underlying primitive type.
    /// This allows the semantic type to be passed directly into methods expecting the raw primitive.
    /// </summary>
    /// <param name="category">The semantic category instance to unwrap.</param>
    public static implicit operator TPrimitive(Semantic<TPrimitive> category) => category.Value;
}