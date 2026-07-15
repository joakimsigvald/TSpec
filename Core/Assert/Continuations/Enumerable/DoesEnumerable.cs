using System.Runtime.CompilerServices;
using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// Continuation that allows assertions to be made on the provided enumerable
/// </summary>
/// <typeparam name="TItem">The type of the elements in the enumerable</typeparam>
public record DoesEnumerable<TItem> : EnumerableConstraint<TItem, DoesEnumerableContinuation<TItem>>
{
    /// <summary>
    /// Asserts that the enumerable contains the given item
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the enumerable</returns>
    public ContinueWith<DoesEnumerableContinuation<TItem>> Contain(
        TItem expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert($"{expected}",
            NotNullAnd(actual => Xunit.Assert.Contains(expected, actual)),
            expectedExpr!, verbalizationStrategy: VerbalizationStrategy.PresentSingularS).And();

    /// <summary>
    /// Asserts that the enumerable contains the given item
    /// </summary>
    /// <param name="filter">The expected criteria</param>
    /// <param name="filterExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the enumerable</returns>
    public ContinueWith<DoesEnumerableContinuation<TItem>> Contain(
        Predicate<TItem> filter, [CallerArgumentExpression(nameof(filter))] string? filterExpr = null)
        => Assert(filterExpr!,
            NotNullAnd(actual => Xunit.Assert.Contains(actual, filter)),
            filterExpr!, verbalizationStrategy: VerbalizationStrategy.PresentSingularS, methodName: "contain items satisfying").And();

    internal override DoesEnumerableContinuation<TItem> Continue() => Create(Actual, ActualExpr);
}