using Moq;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Pipelines;
using TSpec.Internal.Specification;

namespace TSpec;

public abstract partial class Spec<TSUT, TResult> : ITestPipeline<TSUT, TResult>
{
    /// <summary>
    /// Provide any arrangement to the test, which will be applied during test execution in reverse order of where in the test-pipeline it was provided
    /// </summary>
    /// <typeparam name="TValue">The type of the value to arrange</typeparam>
    /// <param name="setup">An action applied to each generated value of the given type</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    public IGivenTestPipeline<TSUT, TResult> Given<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        where TValue : class
    {
        Pipeline.SetDefault(setup, setupExpr!);
        return new GivenTestPipeline<TSUT, TResult>(this);
    }

    internal IGivenTestPipeline<TSUT, TResult> GivenThat(Action customArrangement, string customArrangementExpr)
        => AppendGiven(() =>
        {
            Pipeline.Specification.AddGivenThat(customArrangementExpr);
            customArrangement();
        });

    /// <summary>
    /// Transform any value and use the transformed value as default
    /// </summary>
    /// <typeparam name="TValue">The type of the value to arrange</typeparam>
    /// <param name="transform">A function transforming the default value of the given type</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    public IGivenTestPipeline<TSUT, TResult> Given<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
    {
        Pipeline.SetDefault(transform, transformExpr!);
        return new GivenTestPipeline<TSUT, TResult>(this);
    }

    /// <summary>
    /// Provide a tag to setup some expectation, such as associating it with a value.
    /// </summary>
    /// <typeparam name="TValue">The type of value the tag is associated with</typeparam>
    /// <param name="tag">The tag</param>
    /// <param name="tagExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for setting up an expectation on the tag</returns>
    public IGivenTag<TSUT, TResult, TValue> Given<TValue>(
        Tag<TValue> tag,
        [CallerArgumentExpression(nameof(tag))] string? tagExpr = null)
        => new GivenTag<TSUT, TResult, TValue>(this, tag, tagExpr!);

    /// <summary>
    /// Provide an array of default values, that will be applied in all mocks and auto-generated test-data, where no specific value or setup is given.
    /// It is also mentioned by position so the values can be retrieved by A, ASecond, AThird etc.
    /// </summary>
    /// <typeparam name="TValue">The type of the default values</typeparam>
    /// <param name="defaultValues">The values to use as defaults for the given type</param>
    /// <param name="defaultValuesExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    public IGivenTestPipeline<TSUT, TResult> Given<TValue>(
        TValue[]? defaultValues,
        [CallerArgumentExpression(nameof(defaultValues))] string? defaultValuesExpr = null)
    {
        Pipeline.SetDefault(defaultValues, For.All, defaultValuesExpr!);
        if (defaultValues is not null)
            for (var i = 0; i < defaultValues.Length && i < 5; i++)
                Pipeline.Assign(i, defaultValues[i]);
        return new GivenTestPipeline<TSUT, TResult>(this);
    }

    /// <summary>
    /// Access the mock of the given type to provide mock-setup
    /// </summary>
    /// <typeparam name="TService">The type to mock</typeparam>
    /// <returns>A continuation for providing mock-setup for the given type</returns>
    /// <exception cref="SetupFailed">Thrown when providing arrangement after the test pipeline has been set up</exception>
    public IGivenServiceContinuation<TSUT, TResult, TService> Given<TService>() where TService : class
        => new GivenServiceContinuation<TSUT, TResult, TService>(this);

    /// <summary>
    /// Provide any setup as an action, through the returned continuation
    /// </summary>
    /// <returns>A continuation for providing test data and other arrangement</returns>
    /// <exception cref="SetupFailed">Thrown when providing arrangement after the test pipeline has been set up</exception>
    public IGivenContinuation<TSUT, TResult> Given()
        => new GivenContinuation<TSUT, TResult>(this);

    internal IGivenTestPipeline<TSUT, TResult> GivenDefault<TValue>(
        TValue defaultValue, For scope, string defaultValueExpr)
    {
        Pipeline.SetDefault(defaultValue, scope, defaultValueExpr);
        return new GivenTestPipeline<TSUT, TResult>(this);
    }

    internal IGivenTestPipeline<TSUT, TResult> GivenDefault<TValue>(
        Func<TValue> value, For scope, string defaultValueExpr)
        => PrependGiven(() => Pipeline.SetDefault(value(), scope, defaultValueExpr));

    internal IGivenTestPipeline<TSUT, TResult> Apply<TValue>(
        Action setup,
        string setupExpr,
        bool isCustomExpression = false,
        [CallerMemberName] string? article = null)
        => PrependGiven(() =>
        {
            Pipeline.Specification.AddGiven<TValue>(setupExpr, isCustomExpression, article);
            setup();
        });

    internal IGivenTestPipeline<TSUT, TResult> ApplyMany<TValue>(
        Action setup, [CallerMemberName] string? count = null)
        => PrependGiven(() =>
        {
            Pipeline.Specification.AddGivenCount<TValue>(count!);
            setup();
        });

    internal Mock<TService> GetMock<TService>() where TService : class
        => Pipeline.GetMock<TService>();

    internal void SetupThrows<TService>(Func<Exception> expected)
        => Pipeline.SetupThrows<TService>(expected);

    internal IGivenTestPipeline<TSUT, TResult> AppendGiven(Action given)
    {
        Pipeline.AppendGiven(given);
        return Continue();
    }

    private GivenTestPipeline<TSUT, TResult> PrependGiven(Action given)
    {
        Pipeline.PrependGiven(given);
        return Continue();
    }

    private GivenTestPipeline<TSUT, TResult> Continue() => new(this);
}