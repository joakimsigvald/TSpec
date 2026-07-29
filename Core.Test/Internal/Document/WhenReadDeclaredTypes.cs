using TSpec.Assert;
using TSpec.Internal.Document;
using TSpec.Test.TestData;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// The subject-under-test and return type a spec class declares, which the document states so a
/// developer can tie a requirement to the code it is about.
/// </summary>
public class WhenReadDeclaredTypes : Spec
{
    private sealed class Subject : Spec<MyModel, int>;

    private sealed class SubjectIsAlsoTheResult : Spec<MyModel>;

    private sealed class NoSubject : Spec;

    [Fact]
    public void ThenReadBothTypeArguments()
        => TestIdentity.Declares(typeof(Subject)).Is(("MyModel", "int"));

    /// <summary>Spec&lt;T&gt; is Spec&lt;T, T&gt;, and both lines are stated even so.</summary>
    [Fact]
    public void GivenOneTypeArgument_ThenReadItAsBoth()
        => TestIdentity.Declares(typeof(SubjectIsAlsoTheResult)).Is(("MyModel", "MyModel"));

    /// <summary>
    /// The non-generic Spec is Spec&lt;object, object&gt;, so recognising it has to come first —
    /// otherwise a spec that declares no subject would be documented as having one of type object.
    /// </summary>
    [Fact]
    public void GivenNoTypeArguments_ThenReadNothing()
        => TestIdentity.Declares(typeof(NoSubject)).Is().Null();

    /// <summary>A branch declares what its outer class does, since it derives from it.</summary>
    [Fact]
    public void GivenANestedBranch_ThenReadWhatItInherits()
        => TestIdentity.Declares(typeof(Outer.GivenSomething)).Is(("MyModel", "int"));

    private class Outer : Spec<MyModel, int>
    {
        internal sealed class GivenSomething : Outer;
    }
}
