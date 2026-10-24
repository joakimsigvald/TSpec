using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// Uses/DependingOnOthers.cs uses a type of this package's project, of two packages, of the framework and of
/// its own project.
/// </summary>
public class WhenReadWhatAFileDependsOn : Spec
{
    [Fact]
    public void ThenListTheProjectsAndPackagesWhoseTypesItUses_NeitherTheFrameworkNorItsOwnProject()
        => Depending.DependsOn().Is().EqualTo([".", "P:Microsoft.CodeAnalysis.CSharp", "P:TSpec"]);

    [Fact]
    public void GivenANameItDependsOn_ThenAnswerThatItDoes()
        => Depending.DependsOn("P:TSpec").Is(true);

    [Fact]
    public void GivenANameItDoesNotDependOn_ThenAnswerThatItDoesNot()
        => Depending.DependsOn("P:Microsoft").Is(false);

    [Fact]
    public void GivenANameEndingInStar_ThenAnswerForEveryNameItBegins()
        => Depending.DependsOn("P:Microsoft.*").Is(true);

    [Fact]
    public void GivenOnlyDeclarations_ThenItDependsOnNothing()
        => FileOf("Used.cs").DependsOn().Is().Empty();

    private static SourceFile Depending => FileOf("DependingOnOthers.cs");

    private static SourceFile FileOf(string name)
        => Source.Projects["Test"].Files.Single(file => file.Path == $"Uses/{name}");
}
