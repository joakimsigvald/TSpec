using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TSpec.Architecture.CSharp.Internal;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// A source file of a project, named by its project and its path in it
/// </summary>
public sealed class SourceFile
{
    private readonly Lazy<string> _raw;
    private readonly Lazy<IReadOnlyList<string>> _lines;
    private readonly Lazy<CompilationUnitSyntax> _root;
    private readonly Lazy<IReadOnlyList<DeclaredMember>> _members;
    private readonly Lazy<IReadOnlyList<DeclaredType>> _uses;
    private readonly Lazy<SemanticModel> _model;

    internal SourceFile(string project, string path, string location, CSharpParseOptions options)
    {
        Project = project;
        Path = path;
        Location = location;
        _raw = new(() => File.ReadAllText(location));
        _lines = new(() => LinesOf(Raw));
        _root = new(() => CSharpSyntaxTree.ParseText(Raw, options).GetCompilationUnitRoot());
        _members = new(() => MemberDeclarations.In(this));
        _uses = new(() => BuildTypes.UsedIn(this));
        _model = new(() => CompiledProject.Of(Source.Projects[project]).ModelOf(Root.SyntaxTree));
    }

    /// <summary>
    /// The project the file is compiled into, named as the rules see it
    /// </summary>
    public string Project { get; }

    /// <summary>
    /// The file's path in its project, written with '/': "Rooms/RoomService.cs"
    /// </summary>
    public string Path { get; }

    internal string Location { get; }

    internal SemanticModel Model => _model.Value;

    /// <summary>
    /// The file's text as it is written, for an analysis TSpec does not have. Add one to the file as an
    /// extension property, and keep what is costly to compute in a <c>ConditionalWeakTable</c> keyed by
    /// the file itself: TSpec hands out the same file every time
    /// </summary>
    public string Raw => _raw.Value;

    /// <summary>
    /// The file's lines as an editor counts them: a final line break ends the last line and starts none,
    /// and an empty file has one empty line
    /// </summary>
    public IReadOnlyList<string> Lines => _lines.Value;

    /// <summary>
    /// The types declared at the file's top level, in the ordinal order of their full names; a nested
    /// type stays with its owner
    /// </summary>
    public IReadOnlyList<DeclaredType> Types { get; internal set; } = [];

    /// <summary>
    /// The file's syntax as the build parsed it, in the project's language version and with the symbols
    /// its <c>#if</c> directives test, so code the build left out is not in it
    /// </summary>
    public CompilationUnitSyntax Root => _root.Value;

    /// <summary>
    /// Where a node or token of the file starts and ends
    /// </summary>
    public SourceSpan SpanOf(SyntaxNodeOrToken node) => SpanOf(Root.SyntaxTree.GetLineSpan(node.Span));

    /// <summary>
    /// The members the file declares, in the order they are written: a nested type's and local functions
    /// too
    /// </summary>
    public IReadOnlyList<DeclaredMember> Members => _members.Value;

    /// <summary>
    /// The file's nodes of the given kind, in the order they are written: a directive such as
    /// <c>#pragma</c> among them
    /// </summary>
    /// <typeparam name="T">The kind of node</typeparam>
    public IReadOnlyList<SourceNode<T>> Nodes<T>() where T : SyntaxNode
        => [.. Root.DescendantNodes(IsCode, descendIntoTrivia: true).Where(IsCode).OfType<T>()
            .Select(node => new SourceNode<T>(this, node))];

    /// <summary>
    /// The types of the build the file's code uses, in the ordinal order of their full names: those it
    /// names, and those whose members it names, such as an extension method's class or a constant's type.
    /// A nested type is one of its own. The file's project is compiled to find out, once in a run
    /// </summary>
    public IReadOnlyList<DeclaredType> Uses => _uses.Value;

    /// <summary>
    /// The file as a failure names it: its project and its path in it
    /// </summary>
    public override string ToString() => $"{Project}/{Path}";

    // A documentation comment is prose: a name in its cref is no use of what it names.
    private static bool IsCode(SyntaxNode node) => node is not DocumentationCommentTriviaSyntax;

    private static SourceSpan SpanOf(FileLinePositionSpan span)
        => new(PositionOf(span.StartLinePosition), PositionOf(span.EndLinePosition));

    private static SourcePosition PositionOf(LinePosition position) => new(position.Line + 1, position.Character + 1);

    private static IReadOnlyList<string> LinesOf(string text) => text.Length == 0 ? [""] : [.. ReadLines(new StringReader(text))];

    private static IEnumerable<string> ReadLines(StringReader reader)
    {
        for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
            yield return line;
    }
}
