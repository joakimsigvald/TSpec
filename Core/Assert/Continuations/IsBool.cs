namespace TSpec.Assert.Continuations;

/// <summary>
/// Object that allows assertions to be made on the provided bool
/// </summary>
public record IsBool : Constraint<bool, IsBool>
{
    /// <summary>
    /// Asserts that the value is true
    /// </summary>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsBool> True() => Assert(Ignore.Me, Xunit.Assert.True).And();

    /// <summary>
    /// Asserts that the value is false
    /// </summary>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsBool> False() => Assert(Ignore.Me, Xunit.Assert.False).And();
}