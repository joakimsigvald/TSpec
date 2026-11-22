using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The types in Uses and Exposes, as the files beside them use them.
/// </summary>
public class WhenReadWhatUsesAType : Spec
{
    [Fact]
    public void ThenListTheFilesWhoseCodeUsesIt_InTheOrdinalOrderOfTheirProjectsAndPaths()
        => UsersOf("Test", "Constructed").Is().EqualTo(["Test/Uses/DependingOnOthers.cs", "Test/Uses/Using.cs"]);

    [Fact]
    public void GivenAFileOfAnotherProject_ThenItIsAUserToo()
        => UsersOf(".", "SourceFile").Does().Contain("Test/Uses/Using.cs");

    [Fact]
    public void GivenItsOwnFileNamesIt_ThenThatFileIsAUserToo()
        => UsersOf("Test", "SelfNamed").Is().EqualTo(["Test/Exposes/Exposing.cs"]);

    [Fact]
    public void GivenNoFileNamesIt_ThenItHasNoUser()
        => UsersOf("Test", "Returning").Is().Empty();

    private static IEnumerable<string> UsersOf(string project, string type)
        => Source.Projects[project].AllTypes.Single(declared => declared.Name == type).UsedBy
            .Select(file => file.ToString());
}
