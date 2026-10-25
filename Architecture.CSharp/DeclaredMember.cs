using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp.Internal;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// A member declared in a source file, read from its syntax
/// </summary>
public sealed class DeclaredMember
{
    private readonly MemberDeclaration _declaration;
    private readonly Lazy<IReadOnlyList<ITypeSymbol>> _given;
    private readonly Lazy<IReadOnlyList<ITypeSymbol>> _taken;
    private readonly Lazy<bool> _isOriginal;

    internal DeclaredMember(SourceFile file, MemberDeclaration declaration)
    {
        File = file;
        _declaration = declaration;
        _given = new(() => MemberTypes.GivenBy(this));
        _taken = new(() => MemberTypes.TakenBy(this));
        _isOriginal = new(() => !MemberContracts.ImplementsOrOverrides(file, declaration));
    }

    /// <summary>
    /// The member's name as it is written: "Book", the type's name for a constructor, "this[]" for an
    /// indexer, "operator +" for an operator
    /// </summary>
    public string Name => _declaration.Name;

    /// <summary>
    /// What the member is
    /// </summary>
    public MemberKind Kind => _declaration.Kind;

    /// <summary>
    /// The accessibility the member has in effect: the one it is declared with, or else has by default,
    /// and no more than the types around it allow. A public member of an internal class is internal
    /// </summary>
    public Accessibility Accessibility => _declaration.Accessibility;

    /// <summary>
    /// The modifiers written on the member: public, static, override
    /// </summary>
    public SyntaxTokenList Modifiers => _declaration.Modifiers;

    /// <summary>
    /// The attributes written on the member
    /// </summary>
    public IReadOnlyList<AttributeSyntax> Attributes => [.. _declaration.AttributeLists.SelectMany(list => list.Attributes)];

    /// <summary>
    /// The member's parameters, none for a member that takes none
    /// </summary>
    public IReadOnlyList<ParameterSyntax> Parameters => _declaration.Parameters;

    /// <summary>
    /// The type the member returns or holds, as it is written; none for a constructor, a finalizer or an
    /// enum's value
    /// </summary>
    public TypeSyntax? Type => _declaration.Type;

    /// <summary>
    /// The member's declaration: for a field, the declaration of all its variables; for a primary
    /// constructor, its type's; for a positional property, its parameter
    /// </summary>
    public SyntaxNode Syntax => _declaration.Syntax;

    /// <summary>
    /// The file the member is declared in
    /// </summary>
    public SourceFile File { get; }

    /// <summary>
    /// Whether the member sets its own name and signature: it neither implements an interface's member,
    /// explicitly or not, nor overrides a base type's. Its project is compiled to find out, once in a run
    /// </summary>
    public bool IsOriginal => _isOriginal.Value;

    /// <summary>
    /// The member's lines, each weighed by its indentation level in the file: a line at level n of a member
    /// that starts at level m costs 2^(n - m/2). A primary constructor is measured over its parameter list
    /// </summary>
    public double Size => Sizes.Of(File, File.SpanOf(_declaration.Extent));

    /// <summary>
    /// The type the member returns or holds and the types written inside it, each once, in the order they
    /// are written: a type argument such as <c>Task&lt;T&gt;</c>'s, a tuple's element or an array's. A
    /// named type's own members are not inside it, and a void method gives nothing. Its project is
    /// compiled to find out, once in a run
    /// </summary>
    public IReadOnlyList<ITypeSymbol> Gives() => _given.Value;

    /// <summary>
    /// Whether the type the member returns or holds, or a type written inside it, meets the condition
    /// </summary>
    /// <param name="condition">The condition on a type</param>
    public bool Gives(Func<ITypeSymbol, bool> condition) => Gives().Any(condition);

    /// <summary>
    /// Whether the type the member returns or holds, or a type written inside it, is of the given kind
    /// </summary>
    /// <param name="kind">The kind of type: an enum, an interface</param>
    public bool Gives(TypeKind kind) => Gives(type => type.TypeKind == kind);

    /// <summary>
    /// Whether the type the member returns or holds, or a type written inside it, is the given type,
    /// whatever its type arguments: <c>typeof(IEnumerable&lt;&gt;)</c>. A type it only implements is not it
    /// </summary>
    /// <param name="type">The type</param>
    public bool Gives(Type type) => Gives(symbol => MemberTypes.IsOf(symbol, type));

    /// <summary>
    /// The types of the member's parameters and the types written inside them, each once, in the order
    /// they are written, as <c>Gives()</c> lists them for the type the member returns
    /// </summary>
    public IReadOnlyList<ITypeSymbol> Takes() => _taken.Value;

    /// <summary>
    /// Whether a parameter's type, or a type written inside it, meets the condition
    /// </summary>
    /// <param name="condition">The condition on a type</param>
    public bool Takes(Func<ITypeSymbol, bool> condition) => Takes().Any(condition);

    /// <summary>
    /// Whether a parameter's type, or a type written inside it, is of the given kind
    /// </summary>
    /// <param name="kind">The kind of type: an enum, an interface</param>
    public bool Takes(TypeKind kind) => Takes(type => type.TypeKind == kind);

    /// <summary>
    /// Whether a parameter's type, or a type written inside it, is the given type, whatever its type
    /// arguments
    /// </summary>
    /// <param name="type">The type</param>
    public bool Takes(Type type) => Takes(symbol => MemberTypes.IsOf(symbol, type));

    /// <summary>
    /// The member as a failure names it: its file, the line its name is on and its name
    /// </summary>
    public override string ToString() => $"{File}:{File.SpanOf(_declaration.At).Start.Line} {Name}";
}
