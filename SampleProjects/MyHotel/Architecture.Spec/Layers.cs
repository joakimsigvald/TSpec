using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

public class Layers : TSpec.Spec
{
    private static readonly SourceProjects _projects = Source.Projects.Under("MyHotel");

    [Fact]
    public void ContractDeclaresOnlyPublicTypes()
        => TypesOf("Contract").Where(type => !type.IsPublic).Is().Empty();

    [Fact]
    public void InfraDeclaresNoInterface()
        => TypesOf("Infra").Where(type => type.Kind == DeclaredKind.Interface).Is().Empty();

    private static IEnumerable<DeclaredType> TypesOf(string project)
        => _projects[project].Files.SelectMany(file => file.Types);
}
