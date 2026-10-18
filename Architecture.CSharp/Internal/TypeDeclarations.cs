using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The types a project's files declare at their top level, and those nested in them, read from their syntax:
/// a partial type is one type, declared in each of its files, and so is a type nested in its parts.
/// </summary>
internal static class TypeDeclarations
{
    internal static IEnumerable<DeclaredType> In(IReadOnlyList<SourceFile> files)
        => Declared(files.SelectMany(PartsIn), owner: null);

    private static IEnumerable<Part> PartsIn(SourceFile file)
        => file.Root
            .DescendantNodes(IsNamespaceScope)
            .Select(node => PartOf(node, file))
            .OfType<Part>();

    // A nested type is its owner's, so the walk goes no deeper than the file and its namespaces.
    private static bool IsNamespaceScope(SyntaxNode node) => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax;

    // An extension block is no type of its own: its members are its class's.
    private static IEnumerable<Part> NestedIn(Part part)
        => part.Declaration is TypeDeclarationSyntax type
            ? type.Members.Where(member => member is not ExtensionBlockDeclarationSyntax)
                .Select(member => PartOf(member, part.File)).OfType<Part>()
            : [];

    private static Part? PartOf(SyntaxNode node, SourceFile file)
        => node switch
        {
            TypeDeclarationSyntax type => new(type, type.Identifier, type.TypeParameterList, KindOf(type), file),
            EnumDeclarationSyntax type => new(type, type.Identifier, null, DeclaredKind.Enum, file),
            DelegateDeclarationSyntax type => new(type, type.Identifier, type.TypeParameterList, DeclaredKind.Delegate, file),
            _ => null,
        };

    private static DeclaredKind KindOf(TypeDeclarationSyntax type)
        => type switch
        {
            RecordDeclarationSyntax record when record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
                => DeclaredKind.RecordStruct,
            RecordDeclarationSyntax => DeclaredKind.Record,
            StructDeclarationSyntax => DeclaredKind.Struct,
            InterfaceDeclarationSyntax => DeclaredKind.Interface,
            _ => DeclaredKind.Class,
        };

    private static IEnumerable<DeclaredType> Declared(IEnumerable<Part> parts, DeclaredType? owner)
        => parts.GroupBy(part => part.Identity).Select(type => Declared([.. type], owner));

    private static DeclaredType Declared(IReadOnlyList<Part> parts, DeclaredType? owner)
    {
        var first = parts[0];
        var type = new DeclaredType(
            first.Name, first.TypeParameters, first.Namespace, first.Kind, IsPublic(parts, owner),
            [.. parts.Select(part => part.File).Distinct().OrderBy(file => file.Path, StringComparer.Ordinal)], owner);
        type.Nested = [.. Declared(parts.SelectMany(NestedIn), type).OrderBy(nested => nested.FullName, StringComparer.Ordinal)];
        return type;
    }

    // Public in effect: so declared, inside types that all are.
    private static bool IsPublic(IReadOnlyList<Part> parts, DeclaredType? owner)
        => (owner is null || owner.IsPublic) && parts.Any(part => part.IsPublic);

    /// <summary>
    /// A declaration of a type: all of it, or one part of a partial type.
    /// </summary>
    private sealed record Part(
        MemberDeclarationSyntax Declaration, SyntaxToken Identifier, TypeParameterListSyntax? Parameters,
        DeclaredKind Kind, SourceFile File)
    {
        private static readonly SyntaxKind[] _accessModifiers =
            [SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword, SyntaxKind.PrivateKeyword];

        public string Name => Identifier.Text;

        public IReadOnlyList<string> TypeParameters
            => Parameters is null ? [] : [.. Parameters.Parameters.Select(parameter => parameter.Identifier.Text)];

        public string Namespace
            => string.Join('.', Declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse()
                .Select(declaration => declaration.Name.ToString()));

        // A type nested in an interface is public unless it says otherwise.
        public bool IsPublic
            => Declaration.Modifiers.Any(SyntaxKind.PublicKeyword)
                || (Declaration.Parent is InterfaceDeclarationSyntax && !Declaration.Modifiers.Any(IsAccessModifier));

        // A file-local type belongs to its own file, even where another file declares one by the same name.
        public (string Namespace, string Name, int Arity, SourceFile? File) Identity
            => (Namespace, Name, TypeParameters.Count, Declaration.Modifiers.Any(SyntaxKind.FileKeyword) ? File : null);

        private static bool IsAccessModifier(SyntaxToken modifier) => _accessModifiers.Contains(modifier.Kind());
    }
}
