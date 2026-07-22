using System.Runtime.CompilerServices;
using static Xunit.Assert;

namespace TSpec.Assert.Continuations;

/// <summary>
/// Object that allows assertions to be made on the provided object
/// </summary>
public record IsObject : Constraint<object, IsObject>
{
    /// <summary>
    /// Asserts that the object is of the given type and exposes it, strongly typed, through 'that'
    /// </summary>
    /// <typeparam name="TObject">The type the object is expected to be (subtypes are accepted)</typeparam>
    /// <returns>A continuation exposing the object as <typeparamref name="TObject"/> through 'that'</returns>
    /// <example>
    /// <code>
    /// var encounter = one.Is().A&lt;EncounterComposition&gt;().that;
    /// </code>
    /// </example>
    public ContinueWithThat<IsObject, TObject> A<TObject>()
        => Assert(
            typeof(TObject).Name,
            actual => IsType<TObject>(actual, exactMatch: false),
            typeof(TObject).Name)
        .AndThat(Actual is TObject it ? it : default!);

    /// <summary>
    /// Asserts that the object is of the given type and exposes it, strongly typed, through 'that'
    /// </summary>
    /// <remarks>Synonymous with <see cref="A"/> — reads better for vowel-initial type names (e.g. "is an Order")</remarks>
    /// <typeparam name="TObject">The type the object is expected to be (subtypes are accepted)</typeparam>
    /// <returns>A continuation exposing the object as <typeparamref name="TObject"/> through 'that'</returns>
    /// <example>
    /// <code>
    /// var order = one.Is().An&lt;Order&gt;().that;
    /// </code>
    /// </example>
    public ContinueWithThat<IsObject, TObject> An<TObject>()
        => Assert(
            typeof(TObject).Name,
            actual => IsType<TObject>(actual, exactMatch: false),
            typeof(TObject).Name)
        .AndThat(Actual is TObject it ? it : default!);

    /// <summary>
    /// Asserts that the object is not the same reference as the given object
    /// </summary>
    /// <param name="expected">The object that actual is expected not to be</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the object</returns>
    public ContinueWith<IsObject> Not(
        object? expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(Describe(expected), actual => NotSame(expected, actual), expectedExpr!).And();

    /// <summary>
    /// Asserts that the object is null
    /// </summary>
    /// <returns>A continuation for making further assertions on the object</returns>
    public ContinueWith<IsObject> Null() => Assert(Ignore.Me, Xunit.Assert.Null).And();

    /// <summary>
    /// Asserts that the object is equal to the given object, using the type's equality semantics
    /// </summary>
    /// <remarks>
    /// Compares with <c>Equals</c>. To compare structurally by public fields and properties, use <see cref="Like"/> or <see cref="EquivalentTo"/>
    /// </remarks>
    /// <param name="expected">The expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the object</returns>
    /// <example>
    /// <code>
    /// Then().Result.Is().EqualTo(The&lt;Model&gt;());      // equality semantics of the type
    /// Then().Result.Is().Like(new { Name = "Ada" }); // structural comparison, ignoring type
    /// </code>
    /// </example>
    public ContinueWith<IsObject> EqualTo(
        object expected,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(Describe(expected), actual => Equal(expected, actual), expectedExpr!).And();

    /// <summary>
    /// Asserts that the object is equivalent to the given object with respect to public fields and properties, but ignoring type
    /// (if it walks like a duck...)
    /// </summary>
    /// <remarks>Synonymous with <see cref="EquivalentTo"/></remarks>
    /// <param name="expected">The object that actual is expected to be structurally equivalent to</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the object</returns>
    public ContinueWith<IsObject> Like(
        object expected,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(Describe(expected), actual => Equivalent(expected, actual), expectedExpr!).And();

    /// <summary>
    /// Asserts that the object is equivalent to the given object with respect to public fields and properties, but ignoring type
    /// (if it walks like a duck...)
    /// </summary>
    /// <remarks>Synonymous with <see cref="Like"/></remarks>
    /// <param name="expected">The object that actual is expected to be structurally equivalent to</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the object</returns>
    public ContinueWith<IsObject> EquivalentTo(
        object expected,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(Describe(expected), actual => Equivalent(expected, actual), expectedExpr!).And();
}