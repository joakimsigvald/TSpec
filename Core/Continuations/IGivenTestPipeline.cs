using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// A continuation to provide further arrangement
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
public interface IGivenTestPipeline<TSUT, TResult> : ITestPipeline<TSUT, TResult>
{
    /// <summary>
    /// Access the mock of the given type to provide mock-setup
    /// </summary>
    /// <typeparam name="TService">The type to mock</typeparam>
    /// <returns>A continuation to provide mock-setup for the given type</returns>
    IGivenServiceContinuation<TSUT, TResult, TService> And<TService>() where TService : class;

    /// <summary>
    /// A continuation to provide further arrangement to the test
    /// </summary>
    /// <returns>A continuation for providing test data and other arrangement</returns>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    IGivenContinuation<TSUT, TResult> and { get; }

    /// <summary>
    /// Provide a tag to setup some expectation, such as associating it with a value.
    /// </summary>
    /// <typeparam name="TValue">The type of value the tag is associated with</typeparam>
    /// <param name="tag">The tag</param>
    /// <param name="tagExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for setting up an expectation on the tag</returns>
    IGivenTag<TSUT, TResult, TValue> And<TValue>(
        Tag<TValue> tag,
        [CallerArgumentExpression(nameof(tag))] string? tagExpr = null);
}