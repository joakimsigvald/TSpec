namespace TSpec.Assert.Continuations;

/// <summary>
/// Object that allows assertions to be made on the provided object
/// </summary>
public record HasObject : Constraint<object, HasObject>
{
    /// <summary>
    /// Asserts that the object is of the given type
    /// </summary>
    /// <remarks>
    /// Superseded by <c>Is().A&lt;T&gt;()</c>, which also exposes the object strongly typed through 'that'
    /// and produces a clearer failure message
    /// </remarks>
    /// <typeparam name="TObject">The type of the object to assert on</typeparam>
    /// <returns>A continuation for making further assertions on the value</returns>
    [Obsolete("Use Is().A<T>() (or An<T>()) instead — it asserts the type and exposes the value strongly typed through 'that'.")]
    public ContinueWith<HasObject> Type<TObject>()
        => Assert(Ignore.Me, actual => (actual is TObject).Is().True(), typeof(TObject).Name).And();
}