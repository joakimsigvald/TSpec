using TSpec.Continuations;
using TSpec.Internal.TestData;

namespace TSpec;

public abstract partial class Spec<TSUT, TResult> : ITestPipeline<TSUT, TResult>
{

    /// <summary>
    /// Yields the first value of the given type (a mention).
    /// Repeated mentions of the same type refer to the same auto-generated (or previously provided) value,
    /// and generated values are guaranteed to be unique within the test run.
    /// Using `A` is synonymous to `An`, `The`, `AFirst` and `TheFirst` — pick the alias that reads best.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue A<TValue>() => Pipeline.Mention<TValue>();

    /// <summary>
    /// Yields the first value of the given type (a mention).
    /// Repeated mentions of the same type refer to the same auto-generated (or previously provided) value,
    /// and generated values are guaranteed to be unique within the test run.
    /// Using `An` is synonymous to `A`, `The`, `AFirst` and `TheFirst` — pick the alias that reads best.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue An<TValue>() => Pipeline.Mention<TValue>();

    /// <summary>
    /// Reference an auto-generated, or previously provided value of the given type. 
    /// Using `The` is synonymous to `A` or `An`, but suggest that this value has been provided or referenced earlier in the test pipeline. 
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue The<TValue>() => Pipeline.Mention<TValue>();

    /// <summary>
    /// Reference an auto-generated, or previously provided value by the given tag
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="tag">The tag is used to distinguish between different values. Each tag instance corresponds to one value</param>
    /// <returns>The value associated to the tag</returns>
    protected internal TValue The<TValue>(Tag<TValue> tag) => Pipeline.Mention(tag, tag.Name);

    /// <summary>
    /// Yields the first value of the given type (a mention).
    /// Using `AFirst` is synonymous to `A`, `An`, `The` and `TheFirst`,
    /// but emphasizes the position when the test also mentions ASecond, AThird etc. of the same type.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue AFirst<TValue>() => Pipeline.Mention<TValue>();

    /// <summary>
    /// Yields the first value of the given type (a mention).
    /// Using `TheFirst` is synonymous to `A`, `An`, `The` and `AFirst`,
    /// but emphasizes the position when the test also mentions TheSecond, TheThird etc. of the same type,
    /// and suggests that this value has been provided or referenced earlier in the test pipeline.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue TheFirst<TValue>() => Pipeline.Mention<TValue>();

    /// <summary>
    /// Yields a customized value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value to generate</typeparam>
    /// <param name="setup">An action applied to the generated value before it is returned</param>
    /// <returns>The generated and customized value</returns>
    protected internal TValue A<TValue>(Action<TValue> setup) => Pipeline.Apply<TValue>(setup, 0);

    /// <summary>
    /// Yields a customized value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">An action applied to the value before it is returned</param>
    /// <returns>The generated and customized value</returns>
    protected internal TValue An<TValue>(Action<TValue> setup) => Pipeline.Apply<TValue>(setup, 0);

    /// <summary>
    /// Yields a customized value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">An action applied to the value before it is returned</param>
    /// <returns>The generated and customized value</returns>
    protected internal TValue AFirst<TValue>(Action<TValue> setup)
        => Pipeline.Apply<TValue>(setup, 0);

    /// <summary>
    /// Yields a customized value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">A function transforming the value before it is returned</param>
    /// <returns>The transformed value</returns>
    protected internal TValue A<TValue>(Func<TValue, TValue> transform) => Pipeline.Apply<TValue>(transform, 0);

    /// <summary>
    /// Yields a customized value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">A function transforming the value before it is returned</param>
    /// <returns>The transformed value</returns>
    protected internal TValue An<TValue>(Func<TValue, TValue> transform)
    {
        return Pipeline.Apply<TValue>(transform, 0);
    }

    /// <summary>
    /// Yields a customized value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">A function transforming the value before it is returned</param>
    /// <returns>The transformed value</returns>
    protected internal TValue AFirst<TValue>(Func<TValue, TValue> transform) => Pipeline.Apply<TValue>(transform, 0);

    /// <summary>
    /// Provide a specific value for the first instance of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <returns>The provided value</returns>
    protected internal TValue AFirst<TValue>(TValue value) => Pipeline.Assign(0, value);

    /// <summary>
    /// Provide a specific value of the given type, that can be referenced at different points of the test, with the keywords, A, An or The
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <returns>The provided value</returns>
    protected internal TValue A<TValue>(TValue value) => Pipeline.Assign(0, value);

    /// <summary>
    /// Yields a second value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue TheSecond<TValue>() => Pipeline.Mention<TValue>(1);

    /// <summary>
    /// Yields a second value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue ASecond<TValue>() => Pipeline.Mention<TValue>(1);

    /// <summary>
    /// Yields a customized second value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">An action applied to the value before it is returned</param>
    /// <returns>The generated and customized value</returns>
    protected internal TValue ASecond<TValue>(Action<TValue> setup) => Pipeline.Apply<TValue>(setup, 1);

    /// <summary>
    /// Yields a customized second value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">A function transforming the value before it is returned</param>
    /// <returns>The transformed value</returns>
    protected internal TValue ASecond<TValue>(Func<TValue, TValue> transform) => Pipeline.Apply<TValue>(transform, 1);

    /// <summary>
    /// Provide a specific value for the second instance of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <returns>The provided value</returns>
    protected internal TValue ASecond<TValue>(TValue value) => Pipeline.Assign(1, value);

    /// <summary>
    /// Yields a third value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue TheThird<TValue>() => Pipeline.Mention<TValue>(2);

    /// <summary>
    /// Yields a third value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue AThird<TValue>() => Pipeline.Mention<TValue>(2);

    /// <summary>
    /// Yields a customized third value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">An action applied to the value before it is returned</param>
    /// <returns>The generated and customized value</returns>
    protected internal TValue AThird<TValue>(Action<TValue> setup) => Pipeline.Apply<TValue>(setup, 2);

    /// <summary>
    /// Yields a customized third value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">A function transforming the value before it is returned</param>
    /// <returns>The transformed value</returns>
    protected internal TValue AThird<TValue>(Func<TValue, TValue> transform) => Pipeline.Apply<TValue>(transform, 2);

    /// <summary>
    /// Provide a specific value for the third instance of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <returns>The provided value</returns>
    protected internal TValue AThird<TValue>(TValue value) => Pipeline.Assign(2, value);

    /// <summary>
    /// Yields a fourth value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue TheFourth<TValue>() => Pipeline.Mention<TValue>(3);

    /// <summary>
    /// Yields a fourth value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue AFourth<TValue>() => Pipeline.Mention<TValue>(3);

    /// <summary>
    /// Yields a customized fourth value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">An action applied to the value before it is returned</param>
    /// <returns>The generated and customized value</returns>
    protected internal TValue AFourth<TValue>(Action<TValue> setup) => Pipeline.Apply<TValue>(setup, 3);

    /// <summary>
    /// Yields a customized fourth value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">A function transforming the value before it is returned</param>
    /// <returns>The transformed value</returns>
    protected internal TValue AFourth<TValue>(Func<TValue, TValue> transform) => Pipeline.Apply<TValue>(transform, 3);

    /// <summary>
    /// Provide a specific value for the fourth instance of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <returns>The provided value</returns>
    protected internal TValue AFourth<TValue>(TValue value) => Pipeline.Assign(3, value);

    /// <summary>
    /// Yields a fifth value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue TheFifth<TValue>() => Pipeline.Mention<TValue>(4);

    /// <summary>
    /// Yields a fifth value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue AFifth<TValue>() => Pipeline.Mention<TValue>(4);

    /// <summary>
    /// Yields a customized fifth value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">An action applied to the value before it is returned</param>
    /// <returns>The generated and customized value</returns>
    protected internal TValue AFifth<TValue>(Action<TValue> setup) => Pipeline.Apply<TValue>(setup, 4);

    /// <summary>
    /// Provide a specific value for the fifth instance of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="value">The value to provide</param>
    /// <returns>The provided value</returns>
    protected internal TValue AFifth<TValue>(TValue value) => Pipeline.Assign(4, value);

    /// <summary>
    /// Yields a customized fifth value of the given type
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="transform">A function transforming the value before it is returned</param>
    /// <returns>The transformed value</returns>
    protected internal TValue AFifth<TValue>(Func<TValue, TValue> transform) => Pipeline.Apply<TValue>(transform, 4);

    /// <summary>
    /// Yields a value of the given type that cannot be retrieved again.
    /// Using `Any` is synonymous to `Another` — pick the alias that reads best.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue Any<TValue>() => Pipeline.Mention<TValue>(null);

    /// <summary>
    /// Yields a customized value of the given type that cannot be retrieved again
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">An action applied to the value before it is returned</param>
    /// <returns>The generated and customized value</returns>
    protected internal TValue Any<TValue>(Action<TValue> setup) => Pipeline.Create(setup);

    /// <summary>
    /// Yields a value of the given type that cannot be retrieved again.
    /// Using `Another` is synonymous to `Any` — pick the alias that reads best.
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns>The generated or previously provided value</returns>
    protected internal TValue Another<TValue>() => Pipeline.Mention<TValue>(null);

    /// <summary>
    /// Yields a customized value of the given type that cannot be retrieved again
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="setup">An action applied to the value before it is returned</param>
    /// <returns>The generated and customized value</returns>
    protected internal TValue Another<TValue>(Action<TValue> setup) => Pipeline.Create(setup);

    internal TValue Assign<TValue>(Tag<TValue> tag, TValue value, string tagName) => Pipeline.Assign(tag, value, tagName);

    internal TValue Apply<TValue>(Tag<TValue> tag, Action<TValue> setup, string tagName) => Pipeline.Apply(tag, setup, tagName);

    internal TValue Apply<TValue>(Tag<TValue> tag, Func<TValue, TValue> transform, string tagName) => Pipeline.Apply(tag, transform, tagName);
}