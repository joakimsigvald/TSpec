namespace TSpec.Internal.TestData.Generation;

/// <summary>
/// Exception thrown when type conversion fails during test data generation
/// </summary>
/// <param name="targetType">The type the generated value is converted to</param>
/// <param name="sourceType">The type of the generated value</param>
public class InvalidTypeConversion(Type targetType, Type sourceType) 
    : Exception($"Invalid type conversion: Cannot resolve '{targetType.Name}' using '{sourceType.Name}'. " +
        $"Ensure that '{sourceType.Name}' is assignable to '{targetType.Name}', " +
        $"or can be casted to '{targetType.Name}', " +
        $"or that '{targetType.Name}' has a public constructor accepting '{sourceType.Name}' or an implicitly casted type from '{sourceType.Name}'.");