namespace TSpec.Semantic;

/// <summary>
/// Defines the generation contract for a semantic category type. 
/// Implement this interface alongside <see cref="Semantic{TPrimitive}"/> to instruct the test data generator 
/// on how to create valid, domain-specific values for your type.
/// </summary>
/// <typeparam name="TPrimitive">The underlying primitive data type.</typeparam>
public interface ISemantic<TPrimitive>
{
    /// <summary>
    /// Implemented to provide a deterministic, domain-valid value for the semantic type.
    /// </summary>
    /// <param name="seed">A unique, incrementally increasing number to use as a seed. Guaranteed to be distinct per test run to ensure varied output.</param>
    /// <returns>The generated primitive value.</returns>
    static abstract TPrimitive GenerateValue(int seed);
}