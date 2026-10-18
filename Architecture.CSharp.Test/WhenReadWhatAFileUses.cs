using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The files in Uses: Using.cs uses the types Used.cs declares, and one of the package's.
/// </summary>
public class WhenReadWhatAFileUses : Spec
{
    [Fact]
    public void ThenListTheTypesOfTheBuildItsCodeUses_InTheOrdinalOrderOfTheirFullNames()
        => UsesOf("Using.cs").Select(type => type.Name)
            .Is().EqualTo(["SourceFile", "Constants", "Constructed", "Extensions", "NamedInANameof"]);

    [Fact]
    public void GivenACallOfAnExtensionMethod_ThenItUsesItsClass()
        => UsesOf("Using.cs").Select(type => type.Name).Does().Contain("Extensions");

    [Fact]
    public void GivenAConstant_ThenItUsesItsType()
        => UsesOf("Using.cs").Select(type => type.Name).Does().Contain("Constants");

    [Fact]
    public void GivenANameof_ThenItUsesTheTypeItNames()
        => UsesOf("Using.cs").Select(type => type.Name).Does().Contain("NamedInANameof");

    [Fact]
    public void GivenATypeOfAnotherProject_ThenItUsesThatToo()
        => UsesOf("Using.cs").Single(type => type.Name == "SourceFile").Files[0].Project.Is(".");

    [Fact]
    public void GivenOnlyDeclarations_ThenItUsesNothing()
        => UsesOf("Used.cs").Is().Empty();

    private static IReadOnlyList<DeclaredType> UsesOf(string file)
        => Source.Projects["Test"].Files.Single(source => source.Path == $"Uses/{file}").Uses;
}
