using TSpec.Assert;

namespace TSpec.Test.TestData;

/// <summary>
/// A tag names itself after the variable it is assigned to, so <c>nameof</c> is not needed.
/// The name reaches the reader — it heads the tag's value in the assignments of a failure report —
/// so getting it for free is what makes an unnamed tag a non-issue rather than a trade-off.
/// </summary>
public class WhenNameATag : Spec
{
    private static readonly Tag<int> _staticField = new();
    private readonly Tag<int> _instanceField = new();
    private static readonly Tag<int> _first = new(), _second = new();

    [Fact] public void ThenTakeTheStaticFieldsName() => _staticField.Name.Is("_staticField");

    [Fact] public void ThenTakeTheInstanceFieldsName() => _instanceField.Name.Is("_instanceField");

    /// <summary>Several tags in one declaration are separate variables, and named separately.</summary>
    [Fact]
    public void GivenOneDeclaration_ThenNameEachAfterItsOwnVariable()
    {
        _first.Name.Is("_first");
        _second.Name.Is("_second");
    }

    /// <summary>
    /// The compiler reports the enclosing member, which is the variable only in a field
    /// initializer. A tag built anywhere else takes that member's name, and wants naming.
    /// </summary>
    [Fact]
    public void GivenBuiltOutsideAFieldInitializer_ThenTakeTheEnclosingMembersName()
        => BuiltInAMethod().Name.Is(nameof(BuiltInAMethod));

    [Fact] public void GivenAnExplicitName_ThenUseIt() => new Tag<int>("chosen").Name.Is("chosen");

    /// <summary>Passing null explicitly opts out of the capture, and leaves nothing to name it after.</summary>
    [Fact] public void GivenNoNameAtAll_ThenNumberIt() => new Tag<int>(null).Name.Does().StartWith("Tag_");

    private static Tag<int> BuiltInAMethod() => new();
}
