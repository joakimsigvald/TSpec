using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Placement;

/// <summary>CODING-STANDARD-BACKEND.md 8g and 10a as invariants over the source: our exception types live in
/// an `Exceptions` folder of their sub-domain, named for what happened without the `Exception` suffix, and
/// such a folder holds nothing else. A class is an exception when it derives from Exception, however
/// deep.</summary>
public class ExceptionTypes : TSpec.Spec
{
    private const string Folder = "Exceptions";
    private const string Suffix = "Exception";

    private const string Instead =
        $"name it for what happened, without the suffix, in an {Folder} folder of its sub-domain";

    [Fact]
    public void OurExceptionTypesLiveInAnExceptionsFolderWithoutTheSuffix() =>
        Source.Projects.Has().All(
            project => Classes(project).Has().either.None(MisplacedException).or.Fix(Instead));

    [Fact]
    public void AnExceptionsFolderHoldsOnlyExceptions() =>
        Source.Projects.Has().All(project => Classes(project).Has().None(StrayInTheFolder));

    [Theory]
    [InlineData("Contract/Rooms/RoomNotFound.cs", "RoomNotFound")]
    [InlineData("Contract/Rooms/Exceptions/RoomNotFoundException.cs", "RoomNotFoundException")]
    public void AnExceptionTypeElsewhereOrWithTheSuffixBreaksIt(string file, string name) =>
        Breaks(file, name).Is().True();

    [Theory]
    [InlineData("Contract/Rooms/Exceptions/RoomNotFound.cs", "RoomNotFound")]
    public void OneInAnExceptionsFolderWithoutTheSuffixDoesNot(string file, string name) =>
        Breaks(file, name).Is().False();

    private static IEnumerable<DeclaredType> Classes(SourceProject project) =>
        project.AllTypes.Where(type => type.Kind is DeclaredKind.Class);

    private static bool MisplacedException(DeclaredType type) =>
        IsAnException(type) && Breaks(FileOf(type), type.Name);

    private static bool StrayInTheFolder(DeclaredType type) =>
        InTheFolder(FileOf(type)) && !IsAnException(type);

    private static bool IsAnException(DeclaredType type) => type.DerivesFrom(typeof(Exception));
    private static bool InTheFolder(string file) => file.Split('/') is [.., Folder, _];

    private static bool Breaks(string file, string name) =>
        !InTheFolder(file) || name.EndsWith(Suffix, StringComparison.Ordinal);

    private static string FileOf(DeclaredType type) => type.Files[0].ToString();
}
