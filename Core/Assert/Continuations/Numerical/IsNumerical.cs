namespace TSpec.Assert.Continuations.Numerical;

/// <summary>
/// Base class that allows assertions to be made on the provided numerical
/// </summary>
/// <typeparam name="TContinuation">The concrete type of the assertion continuation, enabling fluent chaining</typeparam>
/// <typeparam name="TActual">The type of the value to assert on</typeparam>
public abstract record IsNumerical<TActual, TContinuation> : IsComparable<TActual, TContinuation>
    where TContinuation : IsNumerical<TActual, TContinuation>, new()
    where TActual : struct, IComparable<TActual>
{
    /// <summary>
    /// Asserts that the value is even (divisible by 2)
    /// </summary>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<TContinuation> Even()
        => Assert(null, NotNullAnd(actual => Xunit.Assert.Equal(0, Convert.ToInt64(actual) % 2))).And();
}