using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// A continuation to mock the behavior of a void method invocation
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
/// <typeparam name="TService">The mocked type</typeparam>
public interface IGivenThatVoidContinuation<TSUT, TResult, TService>
    : IGivenThatCommonContinuation<TSUT, TResult, TService, Void>
    where TService : class
{
}