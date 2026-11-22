using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Dependencies;

public class Layers : TSpec.Spec
{
    [Fact]
    public void ContractDeclaresOnlyPublicTypes() =>
        TypesOf("Contract").Has().All(type => type.IsPublic.Is().True());

    [Fact]
    public void ContractNestsNoType() => TypesOf("Contract").Has().None(type => type.Nested.Count > 0);

    [Fact]
    public void InfraDeclaresNoInterface() =>
        AllTypesOf("Infra").Has().None(type => type.Kind == DeclaredKind.Interface);

    private static IEnumerable<DeclaredType> TypesOf(string project) => Source.Projects[project].Types;
    private static IEnumerable<DeclaredType> AllTypesOf(string project) => Source.Projects[project].AllTypes;
}
