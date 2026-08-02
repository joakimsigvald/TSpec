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

    /// <summary>
    /// Spec&lt;T&gt; is Spec&lt;T, T&gt;, so a return type read from it would only say the subject's
    /// name a second time. It is also the spelling for a subject whose result is not asserted, where
    /// naming a return type at all would be a claim the spec never makes — so the subject is stated
    /// and the return type is left unsaid. Recognising it has to come before Spec&lt;,&gt;.
    /// </summary>
    [Fact]
    public void GivenOneTypeArgument_ThenReadOnlyTheSubject()
        => TestIdentity.Declares(typeof(SubjectIsAlsoTheResult)).Is(("MyModel", null));

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
