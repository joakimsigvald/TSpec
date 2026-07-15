using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// A continuation object to apply additional arrangements to the test
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
public interface IGivenContinuation<TSUT, TResult>
{
    /// <summary>
    /// Provide custom arrangement
    /// </summary>
    /// <param name="customArrangement">An action performing the custom arrangement</param>
    /// <param name="customArrangementExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> That(
        Action customArrangement,
        [CallerArgumentExpression(nameof(customArrangement))] string? customArrangementExpr = null);

    /// <summary>
    /// Provide a value of a given type, that can be mentioned in the test pipeline as A, An, The, or TheFirst.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> A<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null);

    /// <summary>
    /// Provide setup for a value of a given type, that can be mentioned in the test pipeline as A, An, The, or TheFirst.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">The setup to apply</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> A<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        where TValue : class;

    /// <summary>
    /// Provide transform for a value of a given type, that can be mentioned in the test pipeline as A, An, The, or TheFirst.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">The transform to apply</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> A<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Provide a value of a given type, that can be mentioned in the test pipeline as A, An, The, or TheFirst.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> An<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null);

    /// <summary>
    /// Provide setup for a value of a given type, that can be mentioned in the test pipeline as A, An, The, or TheFirst.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">The setup to apply</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> An<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        where TValue : class;

    /// <summary>
    /// Provide transform for a value of a given type, that can be mentioned in the test pipeline as A, An, The, or TheFirst.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">The transform to apply</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> An<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Provide a second value of a given type, that can be mentioned in the test pipeline as ASecond or TheSecond.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> ASecond<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null);

    /// <summary>
    /// Provide setup for a second value of a given type, that can be mentioned in the test pipeline as ASecond or TheSecond.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">The setup to apply</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> ASecond<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        where TValue : class;

    /// <summary>
    /// Provide transform for a second value of a given type, that can be mentioned in the test pipeline as ASecond or TheSecond.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">The transform to apply</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> ASecond<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Provide a third value of a given type, that can be mentioned in the test pipeline as AThird or TheThird.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> AThird<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null);

    /// <summary>
    /// Provide setup for a third value of a given type, that can be mentioned in the test pipeline as AThird or TheThird.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">The setup to apply</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> AThird<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        where TValue : class;

    /// <summary>
    /// Provide transform for a third value of a given type, that can be mentioned in the test pipeline as AThird or TheThird.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">The transform to apply</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> AThird<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Provide a fourth value of a given type, that can be mentioned in the test pipeline as AFourth or TheFourth.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> AFourth<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null);

    /// <summary>
    /// Provide setup for a fourth value of a given type, that can be mentioned in the test pipeline as AFourth or TheFourth.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">The setup to apply</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> AFourth<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        where TValue : class;

    /// <summary>
    /// Provide transform for a fourth value of a given type, that can be mentioned in the test pipeline as AFourth or TheFourth.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">The transform to apply</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> AFourth<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Provide a fifth value of a given type, that can be mentioned in the test pipeline as AFifth or TheFifth.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> AFifth<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null);

    /// <summary>
    /// Provide setup for a fifth value of a given type, that can be mentioned in the test pipeline as AFifth or TheFifth.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">The setup to apply</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> AFifth<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        where TValue : class;

    /// <summary>
    /// Provide transform for a fifth value of a given type, that can be mentioned in the test pipeline as AFifth or TheFifth.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">The transform to apply</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> AFifth<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have zero elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    /// <example>
    /// Arrange that any mentioned collection of int (such as Some&lt;int&gt;()) is empty:
    /// <code>
    /// Given().Zero&lt;int&gt;()
    /// </code>
    /// </example>
    IGivenTestPipeline<TSUT, TResult> Zero<TValue>();

    /// <summary>
    /// Specify that referenced collections of the given type have one element.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> One<TValue>();

    /// <summary>
    /// Specify that referenced collections of the given type have two elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Two<TValue>();

    /// <summary>
    /// Specify that referenced collections of the given type have three elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Three<TValue>();

    /// <summary>
    /// Specify that referenced collections of the given type have four elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Four<TValue>();

    /// <summary>
    /// Specify that referenced collections of the given type have five elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Five<TValue>();

    /// <summary>
    /// Specify that referenced collections of the given type have one element, unless otherwise specified, 
    /// and that the one element is the given value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> One<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have the given elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <param name="values">The elements of the collection</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Some<TValue>(
        IEnumerable<TValue> values,
        [CallerArgumentExpression(nameof(values))] string? valueExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have one element, unless otherwise specified, 
    /// and that the given setup will be applied to the one element.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">The setup to apply</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> One<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have one element, unless otherwise specified, 
    /// and that the given transform will be applied to the one element.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">The transform to apply</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> One<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have two elements, unless otherwise specified, 
    /// and that the given setup will be applied to each of the two elements, given the element and its index.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <param name="setup">An action applied to each element, given the element and its index</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Two<TValue>(
        Action<TValue, int> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have two elements, unless otherwise specified, 
    /// and that the given setup will be applied to the two elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">The setup to apply</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Two<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have two elements, unless otherwise specified, 
    /// and that the given transform will be applied to the two elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">The transform to apply</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Two<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have two elements, unless otherwise specified, 
    /// and that the given transform will be applied to each of the two elements, given the element and its index.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <param name="transform">A function transforming each element, given the element and its index</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Two<TValue>(
        Func<TValue, int, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have three elements, unless otherwise specified, 
    /// and that the given setup will be applied to each of the three elements, given the element and its index.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <param name="setup">An action applied to each element, given the element and its index</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Three<TValue>(
        Action<TValue, int> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have three elements, unless otherwise specified, 
    /// and that the given setup will be applied to the three elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">The setup to apply</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Three<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have three elements, unless otherwise specified, 
    /// and that the given transform will be applied to the three elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">The transform to apply</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Three<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have three elements, unless otherwise specified, 
    /// and that the given transform will be applied to each of the three elements, given the element and its index.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <param name="transform">A function transforming each element, given the element and its index</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Three<TValue>(
        Func<TValue, int, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have four elements, unless otherwise specified, 
    /// and that the given setup will be applied to each of the four elements, given the element and its index.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <param name="setup">An action applied to each element, given the element and its index</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Four<TValue>(
        Action<TValue, int> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have four elements, unless otherwise specified, 
    /// and that the given setup will be applied to the four elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">The setup to apply</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Four<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have four elements, unless otherwise specified, 
    /// and that the given transform will be applied to the four elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">The transform to apply</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Four<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have four elements, unless otherwise specified, 
    /// and that the given transform will be applied to each of the four elements, given the element and its index.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <param name="transform">A function transforming each element, given the element and its index</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Four<TValue>(
        Func<TValue, int, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have five elements, unless otherwise specified, 
    /// and that the given setup will be applied to each of the five elements, given the element and its index.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <param name="setup">An action applied to each element, given the element and its index</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Five<TValue>(
        Action<TValue, int> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have five elements, unless otherwise specified, 
    /// and that the given setup will be applied to the five elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">The setup to apply</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Five<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have five elements, unless otherwise specified, 
    /// and that the given transform will be applied to the five elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">The transform to apply</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Five<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Specify that referenced collections of the given type have five elements, unless otherwise specified, 
    /// and that the given transform will be applied to each of the five elements, given the element and its index.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements in the collection</typeparam>
    /// <param name="transform">A function transforming each element, given the element and its index</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Five<TValue>(
        Func<TValue, int, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);
}