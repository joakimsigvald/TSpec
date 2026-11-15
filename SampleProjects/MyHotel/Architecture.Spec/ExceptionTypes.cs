using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD-BACKEND.md 8g and 10a as invariants over the syntax trees: our exception types
/// live in an `Exceptions` folder of their sub-domain, named for what happened without the `Exception`
/// suffix, and such a folder holds nothing else. A class is an exception when its base is one: a framework's,
/// whose name ends in the suffix, or one of ours - a class in an `Exceptions` folder of any project, which
/// the second fact keeps to exceptions, so a class that derives from one of ours is seen however deep. One
/// fact per project (Sources).</summary>
public class ExceptionTypes : TSpec.Spec
{
    private const string Folder = "Exceptions";
    private const string Suffix = "Exception";

    private const string Instead =
        $"name it for what happened, without the suffix, in an {Folder} folder of its sub-domain";

    /// <summary>Our exception types by name: every class in an `Exceptions` folder of any project.</summary>
    private static readonly IReadOnlySet<string> Ours = new HashSet<string>(
        Sources.Names.SelectMany(Classes).Where(InTheFolder).Select(NameOf), StringComparer.Ordinal);

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void OurExceptionTypesLiveInAnExceptionsFolderWithoutTheSuffix(string project) =>
        Breach(project).Is().Null();

    [Theory, MemberData(nameof(Projects))]
    public void AnExceptionsFolderHoldsOnlyExceptions(string project) =>
        Classes(project).Where(type => InTheFolder(type) && !IsAnException(type.Syntax, Ours))
            .Select(Reported).Is().Empty();

    [Theory]
    [InlineData("class Refused : Exception { }")]
    [InlineData("class Refused(string message) : InvalidOperationException(message);")]
    [InlineData("class Unreachable : System.IO.IOException { }")]
    [InlineData("class Narrower : RoomNotFound { }")]
    public void AClassThatDerivesFromAnExceptionIsOne(string declaration) =>
        IsAnException(ClassOf(declaration), new HashSet<string> { "RoomNotFound" }).Is().True();

    [Theory]
    [InlineData("class Refusal { }")]
    [InlineData("class Refusal : IDisposable { }")]
    [InlineData("class Refusal : Exceptional { }")]
    public void AnotherClassIsNot(string declaration) =>
        IsAnException(ClassOf(declaration), new HashSet<string>()).Is().False();

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

    /// <summary>"Name at file:line" for every exception type of the project that breaks 8g.</summary>
    private static IReadOnlyList<string> Sites(string project) =>
        [.. Classes(project)
            .Where(type => IsAnException(type.Syntax, Ours) && Breaks(type.File.ToString(), NameOf(type)))
            .Select(Reported)];

    private static IEnumerable<SourceNode<ClassDeclarationSyntax>> Classes(string project) =>
        Source.Projects[project].Nodes<ClassDeclarationSyntax>();

    private static bool InTheFolder(SourceNode<ClassDeclarationSyntax> type) =>
        type.File.ToString().Split('/') is [.., Folder, _];

    private static bool IsAnException(ClassDeclarationSyntax type, IReadOnlySet<string> ours) =>
        type.BaseList?.Types.FirstOrDefault()?.Type is { } baseType
        && Rightmost(baseType) is var name
        && (name.EndsWith(Suffix, StringComparison.Ordinal) || ours.Contains(name));

    private static string Rightmost(TypeSyntax type) =>
        type switch
        {
            QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
            AliasQualifiedNameSyntax aliased => aliased.Name.Identifier.ValueText,
            SimpleNameSyntax simple => simple.Identifier.ValueText,
            _ => "",
        };

    private static bool Breaks(string file, string name) =>
        file.Split('/') is not [.., Folder, _] || name.EndsWith(Suffix, StringComparison.Ordinal);

    private static string NameOf(SourceNode<ClassDeclarationSyntax> type) => type.Syntax.Identifier.ValueText;

    private static string Reported(SourceNode<ClassDeclarationSyntax> type) =>
        $"{NameOf(type)} at {type.File}:{type.File.Line(type.Syntax)}";

    /// <summary>The class a snippet declares.</summary>
    private static ClassDeclarationSyntax ClassOf(string declaration) =>
        (ClassDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(declaration).Members[0];
}
