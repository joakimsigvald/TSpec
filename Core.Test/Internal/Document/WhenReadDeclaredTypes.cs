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

    /// <summary>
    /// A tuple reads as C# writes it, with the element names the spec declared. The compiler keeps
    /// them on the class that names the base, as one list: each tuple's own names, then those of the
    /// tuples inside it.
    /// </summary>
    [Theory]
    [InlineData(typeof(NamedResult), "MyModel", "(int Id, string Name)")]
    [InlineData(typeof(UnnamedResult), "MyModel", "(int, string)")]
    [InlineData(typeof(PartlyNamedResult), "MyModel", "(int Id, string)")]
    [InlineData(typeof(NamedInsideAGeneric), "MyModel", "IReadOnlyList<(int Id, string Name)>")]
    [InlineData(typeof(NamedInsideANamedTuple), "MyModel", "(int A, (int B, int C) D)")]
    [InlineData(typeof(NamedSubject), "(int X, int Y)", "int")]
    [InlineData(typeof(NamedSubjectIsAlsoTheResult), "(int X, int Y)", "(int X, int Y)")]
    [InlineData(typeof(InheritsANamedResult), "MyModel", "(int Id, string Name)")]
    [InlineData(typeof(NamedAfterALongTuple), "(int, int, int, int, int, int, int, int)", "(int A, int B)")]
    public void GivenATuple_ThenReadItAsDeclared(Type testClass, string subject, string result)
        => Declares(testClass).Is((subject, result));

    private sealed class NamedResult : Spec<MyModel, (int Id, string Name)>;
    private sealed class UnnamedResult : Spec<MyModel, (int, string)>;
    private sealed class PartlyNamedResult : Spec<MyModel, (int Id, string)>;
    private sealed class NamedInsideAGeneric : Spec<MyModel, IReadOnlyList<(int Id, string Name)>>;
    private sealed class NamedInsideANamedTuple : Spec<MyModel, (int A, (int B, int C) D)>;
    private sealed class NamedSubject : Spec<(int X, int Y), int>;
    private sealed class NamedSubjectIsAlsoTheResult : Spec<(int X, int Y)>;
    private abstract class NamedResultBase : Spec<MyModel, (int Id, string Name)>;
    private sealed class InheritsANamedResult : NamedResultBase;
    private sealed class NamedAfterALongTuple : Spec<(int, int, int, int, int, int, int, int), (int A, int B)>;

    private class Outer : Spec<MyModel, int>
    {
        internal sealed class GivenSomething : Outer;
    }
}
