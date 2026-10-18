using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The folders of this project: Uses/UsingOtherFolders.cs uses a type of Declared and one at the project's
/// root, and the other files in Uses use the types beside them and one of the package.
/// </summary>
public class WhenReadTheFolders : Spec
{
    [Fact]
    public void ThenListTheFoldersDirectlyInTheProjectsOwnThatHoldItsFiles_InOrdinalOrder()
        => Folders.Keys.Is().EqualTo(["Declared", "Exposes", "Internal", "Uses"]);

    [Fact]
    public void GivenAFileUsesATypeOfAnotherFolder_ThenItsFolderUsesThatFolder()
        => Folders["Uses"].Does().Contain("Declared");

    [Fact]
    public void GivenAFileUsesATypeOfItsOwnFolder_AtTheRootOrOfAnotherProject_ThenItsFolderUsesNoMoreForIt()
        => Folders["Uses"].Is().EqualTo(["Declared"]);

    [Fact]
    public void GivenNoFileUsesAnotherFolder_ThenTheFolderUsesNone()
        => Folders["Declared"].Is().Empty();

    [Fact]
    public void ThenAssertWhatEachMayUse()
        => Folders.Is().Within(folder => folder switch
        {
            "Uses" => ["Declared"],
            _ => [],
        });

    private static DependencyGraph Folders => Source.Projects["Test"].Folders;
}
