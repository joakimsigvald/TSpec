namespace TSpec.Assert.Continuations;

/// <summary>
/// Object that allows assertions to be made on the provided object
/// </summary>
public record HasObject : Constraint<object, HasObject>
{
    /// <summary>
    /// Asserts that the object is of the given type
    /// </summary>
    /// <typeparam name="TObject">The type of the object to assert on</typeparam>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<HasObject> Type<TObject>()
        => Assert(Ignore.Me, actual => (actual is TObject).Is().True(), typeof(TObject).Name).And();
}