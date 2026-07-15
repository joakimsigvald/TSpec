using System.Runtime.CompilerServices;
using TSpec.Assert.Continuations;
using TSpec.Internal.Specification;

namespace TSpec.Assert;

/// <summary>
/// Fluent assertions on miscellaneous types
/// </summary>
public static class AssertionExtensions
{
    /// <summary>
    /// Verify that actual object is same reference as expected and return continuation for further assertions of the object
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the object</returns>
    public static ContinueWith<IsNullableStruct<TValue>> Is<TValue>(
        this TValue? actual,
        TValue? expected,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        where TValue : struct
        => actual.Is(actualExpr: actualExpr).Value(expected, expectedExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsBool Is(
        this bool actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => IsBool.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given object
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsNullableStruct<TValue> Is<TValue>
        (this TValue? actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        where TValue : struct
        => IsNullableStruct<TValue>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given comparable
    /// </summary>
    /// <typeparam name="TValue">The type of the value to assert on</typeparam>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsComparable<TValue> Is<TValue>(
        this TValue actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        where TValue : IComparable<TValue>
        => IsComparable<TValue>.Create(actual, actualExpr!);

    /// <summary>
    /// Verify that the value satisfies a given condition
    /// </summary>
    /// <typeparam name="TActual">The type of the value to assert on</typeparam>
    /// <param name="actual">The value to assert on</param>
    /// <param name="condition">The condition that the value is expected to satisfy</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="conditionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWithActual<TActual> Has<TActual>(
        this TActual actual, Func<TActual, bool> condition,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null,
        [CallerArgumentExpression(nameof(condition))] string? conditionExpr = null)
    {
        DoesValue<TActual>.Create(actual, actualExpr!).Have(condition, conditionExpr!);
        return new(actual);
    }

    /// <summary>
    /// Provide actual of any type to continue the chain of assertions on the new value
    /// </summary>
    /// <typeparam name="TActual">The type of the value to assert on</typeparam>
    /// <typeparam name="TContinuation">The concrete type of the assertion continuation, enabling fluent chaining</typeparam>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actual">The value to assert on</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static TActual And<TActual, TContinuation>(
        this ContinueWith<TContinuation> _,
        TActual actual,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        where TContinuation : Constraint
    {
        actualExpr.AssertNoTrainwreck();
        SpecificationContext.Current.AddThen();
        SpecificationContext.Current.SetSubject(actualExpr);
        return actual;
    }

    internal static void HasMessage(this Xunit.Sdk.XunitException ex, string error, string? spec = null)
    {
        ex.Message.NormalizeLineEndings().Is(error.NormalizeLineEndings());
        if (spec is null)
            return;

        ex.HasInnerMessage(spec);
    }

    private static void HasInnerMessage(this Xunit.Sdk.XunitException ex, string expected)
        => SplitInnerExceptionMessage(ex)[0].NormalizeLineEndings()
            .Is($"\n{expected.NormalizeLineEndings()}\n");

    /// <summary>
    /// Verify that the exception message contains the expected assignments section
    /// </summary>
    /// <param name="ex">The exception to assert on</param>
    /// <param name="expected">The expected value</param>
    internal static void HasAssignments(this Xunit.Sdk.XunitException ex, string expected)
    {
        SplitInnerExceptionMessage(ex).Has().TwoItems().that.second
            .NormalizeLineEndings().Is($"\n{expected.NormalizeLineEndings()}\n");
    }

    private static string[] SplitInnerExceptionMessage(Xunit.Sdk.XunitException ex)
    {
        var innerEx = ex.InnerException;
        innerEx!.Is().not.Null();
        return innerEx!.Message.Split("----");
    }
}