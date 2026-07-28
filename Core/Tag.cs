using System.Runtime.CompilerServices;

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
/// Declare tags as static readonly class fields — <c>static readonly Tag&lt;int&gt; _age = new();</c>
/// — which is the only place the compiler can name them after their variable. A tag declared as a
/// local takes its method's name instead, and needs naming: <c>Tag&lt;int&gt; age = new(nameof(age));</c>.
/// Names must be unique within a test; a clash throws <see cref="SetupFailed"/>.
/// </remarks>
/// <param name="name">Supplied by the compiler for a field; provide it for a tag declared anywhere else</param>
public class Tag<TValue>([CallerMemberName] string? name = null)
{
    /// <summary>
    /// Identifies the tag's value in a failure report. It never reaches the specification text,
    /// which names a value from the expression that referenced it.
    /// </summary>
    public string Name { get; init; } = name ?? $"Tag_{Next()}";

    private static int _nextNumber;

    private static int Next() => Interlocked.Increment(ref _nextNumber);
}
