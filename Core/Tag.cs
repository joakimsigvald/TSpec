namespace TSpec;

/// <summary>
/// A tag is an object that can be associated with a value of any type, given by the generic parameter.
/// The associated value can be defined and/or referenced in a similar manner to A, The, AFirst, ASecond etc.
/// Tags are useful to make tests more expressive and readable.
/// Example: Instead of using `AFirst`, `ASecond` and `AThird` to reference three different ints,
/// you can declare three tags, such as `age`, `length` and `size` and reference the values using 
/// `The(age)`, `The(length)` and `The(size)`.
/// </summary>
/// <typeparam name="TValue">The type of the value associated to the tag</typeparam>
/// <remarks>
/// Please, provide the name of the tag, using nameof([variable name]),
/// otherwise a unique name will be auto-generated of the form Tag_[number]
/// </remarks>
/// <param name="name">The name of the tag, used in diagnostic output. If omitted, a unique name is auto-generated</param>
public class Tag<TValue>(string? name = null)
{
    /// <summary>
    /// The name of the tag is used when printing the tag value when a test fails
    /// </summary>
    public string Name { get; init; } = name ?? $"Tag_{Next()}";

    private static int _nextNumber;

    private static int Next() => Interlocked.Increment(ref _nextNumber);
}