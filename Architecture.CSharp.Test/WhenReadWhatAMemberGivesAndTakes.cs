using Microsoft.CodeAnalysis;
using TSpec.Architecture;
using TSpec.Assert;
using TSpec.Architecture.CSharp.Test.Signatures;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The members in Signatures/Signing.cs, read from the build.
/// </summary>
public class WhenReadWhatAMemberGivesAndTakes : Spec
{
    [Fact]
    public void GivenItReturnsAnEnum_ThenItGivesAnEnum()
        => Member("Plain").Gives(TypeKind.Enum).Is(true);

    [Fact]
    public void GivenItHoldsAnEnum_ThenItGivesAnEnum()
        => Member("Held").Gives(TypeKind.Enum).Is(true);

    [Fact]
    public void GivenAnEnumWrittenInsideItsType_ThenItGivesAnEnum()
        => new[] { "Listed", "Paired", "Arrayed", "Awaited", "Maybe" }
            .Has().All(name => Member(name).Gives(TypeKind.Enum));

    [Fact]
    public void GivenARecordWithAnEnumInside_ThenItGivesNoEnum()
        => Member("Wrapped").Gives(TypeKind.Enum).Is(false);

    [Fact]
    public void GivenAVoidMethod_ThenItGivesNothing()
        => Member("Nothing").Gives(_ => true).Is(false);

    [Fact]
    public void GivenACondition_ThenTestEveryTypeWrittenInside()
        => Member("AwaitedPair").Gives(type => type.IsTupleType).Is(true);

    [Fact]
    public void GivenAType_ThenItGivesItWhateverItsTypeArguments()
        => Member("Sequence").Gives(typeof(IEnumerable<>)).Is(true);

    [Fact]
    public void GivenATypeOfTheBuild_ThenItGivesIt()
        => Member("Wrapped").Gives(typeof(Booking)).Is(true);

    [Fact]
    public void GivenATypeThatOnlyImplementsTheOneAskedFor_ThenItDoesNotGiveIt()
        => Member("Listing").Gives(typeof(IEnumerable<>)).Is(false);

    [Fact]
    public void GivenAParameterWithAnEnumInside_ThenItTakesAnEnum()
        => Member("Taking").Takes(TypeKind.Enum).Is(true);

    [Fact]
    public void GivenATypeAParameterHas_ThenItTakesIt()
        => Member("Taking").Takes(typeof(List<>)).Is(true);

    [Fact]
    public void GivenACondition_ThenTestEveryParameter()
        => Member("Taking").Takes(type => type.SpecialType == SpecialType.System_Int32).Is(true);

    [Fact]
    public void GivenARecordsPositionalParameter_ThenItsConstructorTakesItsType()
        => Member("Booking").Takes(TypeKind.Enum).Is(true);

    [Fact]
    public void GivenNoParameters_ThenItTakesNothing()
        => Member("Plain").Takes(_ => true).Is(false);

    [Fact]
    public void GivenWhatItReturns_ThenItDoesNotTakeIt()
        => Member("Plain").Takes(TypeKind.Enum).Is(false);

    [Fact]
    public void ThenListWhatItGives_InTheOrderItIsWritten()
        => Member("Paired").Gives().Select(type => type.Name).Is().EqualTo(["ValueTuple", "Int32", "Status"]);

    [Fact]
    public void GivenATypeWrittenTwice_ThenListItOnce()
        => Member("Twice").Gives().Select(type => type.Name).Is().EqualTo(["ValueTuple", "Status"]);

    [Fact]
    public void GivenAVoidMethod_ThenListNothingItGives()
        => Member("Nothing").Gives().Is().Empty();

    [Fact]
    public void ThenListWhatItTakes_InTheOrderItIsWritten()
        => Member("Taking").Takes().Select(type => type.Name).Is().EqualTo(["List", "Status", "Int32"]);

    [Fact]
    public void GivenTwoParametersOfOneType_ThenListItOnce()
        => Member("TakingTwice").Takes().Select(type => type.Name).Is().EqualTo(["Status"]);

    private static DeclaredMember Member(string name)
        => Source.Projects["Test"].Members.Single(member => member.File.Path == "Signatures/Signing.cs" && member.Name == name);
}
