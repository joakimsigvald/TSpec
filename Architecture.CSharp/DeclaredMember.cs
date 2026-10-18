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

    internal DeclaredMember(SourceFile file, MemberDeclaration declaration)
    {
        File = file;
        _declaration = declaration;
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
    /// The member as a failure names it: its file, the line its name is on and its name
    /// </summary>
    public override string ToString() => $"{File}:{File.SpanOf(_declaration.At).Start.Line} {Name}";
}
