namespace TSpec.Continuations;

/// <summary>
/// A continuation returned by From&lt;TSource&gt;(), allowing the generated source values to be constrained
/// with the extension methods StartingAt and Spaced (for numeric source types), or provide further arrangement.
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
/// <typeparam name="TSource">The source type whose generated values can be constrained</typeparam>
public interface IUsingFromContinuation<TSUT, TResult, TSource> : IUsingTestPipeline<TSUT, TResult>
{
}
