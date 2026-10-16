using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The types a project's files declare at their top level, read from their syntax: a partial type is one
/// type, declared in each of its files.
/// </summary>
internal static class TypeDeclarations
{
    internal static IEnumerable<DeclaredType> In(IReadOnlyList<SourceFile> files, CSharpParseOptions options)
        => files
            .SelectMany(file => PartsIn(file, options))
            .GroupBy(part => part.Identity)
            .Select(Declared);

    private static IEnumerable<Part> PartsIn(SourceFile file, CSharpParseOptions options)
        => CSharpSyntaxTree.ParseText(file.Raw, options).GetRoot()
            .DescendantNodes(IsNamespaceScope)
            .Select(node => PartOf(node, file))
            .OfType<Part>();

    // A nested type stays with its owner, so the walk goes no deeper than the file and its namespaces.
    private static bool IsNamespaceScope(SyntaxNode node) => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax;

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

    private static DeclaredType Declared(IEnumerable<Part> parts)
    {
        var first = parts.First();
        return new(first.Name, first.TypeParameters, first.Namespace, first.Kind, parts.Any(part => part.IsPublic),
            [.. parts.Select(part => part.File).Distinct().OrderBy(file => file.Path, StringComparer.Ordinal)]);
    }

    /// <summary>
    /// A declaration of a type: all of it, or one part of a partial type.
    /// </summary>
    private sealed record Part(
        MemberDeclarationSyntax Declaration, SyntaxToken Identifier, TypeParameterListSyntax? Parameters,
        DeclaredKind Kind, SourceFile File)
    {
        public string Name => Identifier.Text;

        public IReadOnlyList<string> TypeParameters
            => Parameters is null ? [] : [.. Parameters.Parameters.Select(parameter => parameter.Identifier.Text)];

        public string Namespace
            => string.Join('.', Declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse()
                .Select(declaration => declaration.Name.ToString()));

        public bool IsPublic => Declaration.Modifiers.Any(SyntaxKind.PublicKeyword);

        // A file-local type belongs to its own file, even where another file declares one by the same name.
        public (string Namespace, string Name, int Arity, SourceFile? File) Identity
            => (Namespace, Name, TypeParameters.Count, Declaration.Modifiers.Any(SyntaxKind.FileKeyword) ? File : null);
    }
}
