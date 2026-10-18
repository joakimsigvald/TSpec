using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

public class Layers : TSpec.Spec
{
    [Fact]
    public void ContractDeclaresOnlyPublicTypes()
        => TypesOf("Contract").Has().All(type => type.IsPublic);

    [Fact]
    public void InfraDeclaresNoInterface()
        => TypesOf("Infra").Has().None(type => type.Kind == DeclaredKind.Interface);

    private static IEnumerable<DeclaredType> TypesOf(string project)
        => Source.Projects[project].Files.SelectMany(file => file.Types);
}
