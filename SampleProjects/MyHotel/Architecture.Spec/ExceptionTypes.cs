using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD-BACKEND.md 8g and 10a as invariants over the source: our exception types live in
/// an `Exceptions` folder of their sub-domain, named for what happened without the `Exception` suffix, and
/// such a folder holds nothing else. A class is an exception when it derives from Exception, however deep.
/// One fact per project (Sources).</summary>
public class ExceptionTypes : TSpec.Spec
{
    private const string Folder = "Exceptions";
    private const string Suffix = "Exception";

    private const string Instead =
        $"name it for what happened, without the suffix, in an {Folder} folder of its sub-domain";

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void OurExceptionTypesLiveInAnExceptionsFolderWithoutTheSuffix(string project) =>
        Breach(project).Is().Null();

    [Theory, MemberData(nameof(Projects))]
    public void AnExceptionsFolderHoldsOnlyExceptions(string project) =>
        Classes(project).Where(type => InTheFolder(FileOf(type)) && !IsAnException(type))
            .Select(Reported).Is().Empty();

    [Theory]
    [InlineData("Contract/Rooms/RoomNotFound.cs", "RoomNotFound")]
    [InlineData("Contract/Rooms/Exceptions/RoomNotFoundException.cs", "RoomNotFoundException")]
    public void AnExceptionTypeElsewhereOrWithTheSuffixBreaksIt(string file, string name) =>
        Breaks(file, name).Is().True();

    [Theory]
    [InlineData("Contract/Rooms/Exceptions/RoomNotFound.cs", "RoomNotFound")]
    public void OneInAnExceptionsFolderWithoutTheSuffixDoesNot(string file, string name) =>
        Breaks(file, name).Is().False();

    private static string? Breach(string project) =>
        Sites(project) is [var first, ..] and var sites
            ? $"{sites.Count} exception types break 8g, the first {first}; {Instead}"
            : null;

    /// <summary>"Name in file" for every exception type of the project that breaks 8g.</summary>
    private static IReadOnlyList<string> Sites(string project) =>
        [.. Classes(project)
            .Where(type => IsAnException(type) && Breaks(FileOf(type), type.Name))
            .Select(Reported)];

    private static IEnumerable<DeclaredType> Classes(string project) =>
        Source.Projects[project].AllTypes.Where(type => type.Kind is DeclaredKind.Class);

    private static bool IsAnException(DeclaredType type) => type.DerivesFrom(typeof(Exception));
    private static bool InTheFolder(string file) => file.Split('/') is [.., Folder, _];

    private static bool Breaks(string file, string name) =>
        !InTheFolder(file) || name.EndsWith(Suffix, StringComparison.Ordinal);

    private static string FileOf(DeclaredType type) => type.Files[0].ToString();
    private static string Reported(DeclaredType type) => $"{type.Name} in {FileOf(type)}";
}
