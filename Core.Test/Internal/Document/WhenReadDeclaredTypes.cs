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

    /// A spec that both acts on its subject and yields a result, which is the ordinary case.
    private static (string?, string?)? Declares(Type testClass)
        => TestIdentity.Declares(testClass, actsOnSubject: true, yieldsResult: true);

    [Fact]
    public void ThenReadBothTypeArguments()
        => Declares(typeof(Subject)).Is(("MyModel", "int"));

    /// <summary>Spec&lt;T&gt; is Spec&lt;T, T&gt;, and a spec using it as both states both.</summary>
    [Fact]
    public void GivenOneTypeArgument_ThenReadItAsBoth()
        => Declares(typeof(SubjectIsAlsoTheResult)).Is(("MyModel", "MyModel"));

    /// <summary>
    /// The non-generic Spec is Spec&lt;object, object&gt;, so recognising it has to come first —
    /// otherwise a spec that declares no subject would be documented as having one of type object.
    /// </summary>
    [Fact]
    public void GivenNoTypeArguments_ThenReadNothing()
        => Declares(typeof(NoSubject)).Is().Null();

    /// <summary>A branch declares what its outer class does, since it derives from it.</summary>
    [Fact]
    public void GivenANestedBranch_ThenReadWhatItInherits()
        => Declares(typeof(Outer.GivenSomething)).Is(("MyModel", "int"));

    /// <summary>
    /// A type argument states something only where the spec uses it in that capacity. An act taking
    /// no subject leaves a generated value nothing ever reads, and naming it "subject under test"
    /// would document a value the requirement is not about.
    /// </summary>
    [Fact]
    public void GivenTheActTakesNoSubject_ThenReadOnlyTheReturnType()
        => TestIdentity.Declares(typeof(Subject), actsOnSubject: false, yieldsResult: true)
            .Is((null, "int"));

    /// <summary>An act yielding nothing has no return type to state, whatever TResult says.</summary>
    [Fact]
    public void GivenTheActYieldsNothing_ThenReadOnlyTheSubject()
        => TestIdentity.Declares(typeof(Subject), actsOnSubject: true, yieldsResult: false)
            .Is(("MyModel", null));

    /// <summary>Neither used is the same as declaring neither, so the document says nothing.</summary>
    [Fact]
    public void GivenNeitherIsUsed_ThenReadNothing()
        => TestIdentity.Declares(typeof(Subject), actsOnSubject: false, yieldsResult: false)
            .Is().Null();

    private class Outer : Spec<MyModel, int>
    {
        internal sealed class GivenSomething : Outer;
    }
}
