using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The members a file declares, read from its syntax in the order they are written.
/// </summary>
internal static class MemberDeclarations
{
    internal static IReadOnlyList<DeclaredMember> In(SourceFile file)
        => [.. file.Root.DescendantNodes().SelectMany(DeclaredBy).Select(InEffect)
            .Select(declaration => new DeclaredMember(file, declaration))];

    // A primary constructor's declaration is its type's, so the types around it start with that one. An
    // extension block has no accessibility of its own: it is as accessible as its class.
    private static MemberDeclaration InEffect(MemberDeclaration declaration)
        => declaration with
        {
            Accessibility = declaration.Syntax.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>()
                .Where(type => type is not ExtensionBlockDeclarationSyntax)
                .Select(type => AccessibilityOf(type.Modifiers, ByDefault(type)))
                .Aggregate(declaration.Accessibility, Narrower),
        };

    // Protected and internal reach different code, so what both allow is the two together.
    private static Accessibility Narrower(Accessibility one, Accessibility other)
        => (one, other) switch
        {
            _ when one == other => one,
            (Accessibility.Public, _) => other,
            (_, Accessibility.Public) => one,
            (Accessibility.Private, _) or (_, Accessibility.Private) => Accessibility.Private,
            (Accessibility.ProtectedOrInternal, _) => other,
            (_, Accessibility.ProtectedOrInternal) => one,
            _ => Accessibility.ProtectedAndInternal,
        };

    private static IEnumerable<MemberDeclaration> DeclaredBy(SyntaxNode node)
        => node switch
        {
            MethodDeclarationSyntax m => [Of(m, MemberKind.Method, m.Identifier, m.ReturnType, m.ParameterList)],
            ConstructorDeclarationSyntax c => [Of(c, MemberKind.Constructor, c.Identifier, null, c.ParameterList)],
            DestructorDeclarationSyntax d
                => [Of(d, MemberKind.Destructor, d.Identifier, null, d.ParameterList) with { Name = $"~{d.Identifier.Text}" }],
            PropertyDeclarationSyntax p => [Of(p, MemberKind.Property, p.Identifier, p.Type, null)],
            IndexerDeclarationSyntax i
                => [Of(i, MemberKind.Indexer, i.ThisKeyword, i.Type, i.ParameterList) with { Name = "this[]" }],
            FieldDeclarationSyntax f => Variables(f, MemberKind.Field, f.Declaration),
            EventFieldDeclarationSyntax e => Variables(e, MemberKind.Event, e.Declaration),
            EventDeclarationSyntax e => [Of(e, MemberKind.Event, e.Identifier, e.Type, null)],
            OperatorDeclarationSyntax o
                => [Of(o, MemberKind.Operator, o.OperatorKeyword, o.ReturnType, o.ParameterList)
                    with { Name = $"operator {o.OperatorToken.Text}" }],
            ConversionOperatorDeclarationSyntax c
                => [Of(c, MemberKind.Conversion, c.ImplicitOrExplicitKeyword, c.Type, c.ParameterList)
                    with { Name = $"{c.ImplicitOrExplicitKeyword.Text} operator {c.Type}" }],
            EnumMemberDeclarationSyntax e => [Of(e, MemberKind.EnumMember, e.Identifier, null, null)],
            DelegateDeclarationSyntax d => [Of(d, MemberKind.Delegate, d.Identifier, d.ReturnType, d.ParameterList)],
            LocalFunctionStatementSyntax l => [LocalFunction(l)],
            TypeDeclarationSyntax { ParameterList: { } parameters } type => Primary(type, parameters),
            _ => [],
        };

    private static MemberDeclaration Of(
        MemberDeclarationSyntax member, MemberKind kind, SyntaxToken at, TypeSyntax? type, BaseParameterListSyntax? parameters)
        => new(member, kind, at, type, [.. parameters?.Parameters ?? []], member.Modifiers, member.AttributeLists,
            AccessibilityOf(member.Modifiers, ByDefault(member)));

    private static IEnumerable<MemberDeclaration> Variables(
        BaseFieldDeclarationSyntax field, MemberKind kind, VariableDeclarationSyntax declaration)
        => declaration.Variables.Select(variable => Of(field, kind, variable.Identifier, declaration.Type, null));

    private static MemberDeclaration LocalFunction(LocalFunctionStatementSyntax function)
        => new(function, MemberKind.LocalFunction, function.Identifier, function.ReturnType, [.. function.ParameterList.Parameters],
            function.Modifiers, function.AttributeLists, Accessibility.Private);

    // A record's positional parameters are its properties too; a class's or struct's are only the constructor's.
    private static IEnumerable<MemberDeclaration> Primary(TypeDeclarationSyntax type, ParameterListSyntax parameters)
        => [Constructor(type, parameters), .. type is RecordDeclarationSyntax ? parameters.Parameters.Select(Positional) : []];

    private static MemberDeclaration Constructor(TypeDeclarationSyntax type, ParameterListSyntax parameters)
        => new(type, MemberKind.Constructor, type.Identifier, null, [.. parameters.Parameters], default,
            Targeting(type.AttributeLists, SyntaxKind.MethodKeyword), Accessibility.Public);

    private static MemberDeclaration Positional(ParameterSyntax parameter)
        => new(parameter, MemberKind.Property, parameter.Identifier, parameter.Type, [], default,
            Targeting(parameter.AttributeLists, SyntaxKind.PropertyKeyword), Accessibility.Public);

    private static IEnumerable<AttributeListSyntax> Targeting(SyntaxList<AttributeListSyntax> lists, SyntaxKind target)
        => lists.Where(list => list.Target?.Identifier.IsKind(target) == true);

    private static Accessibility AccessibilityOf(SyntaxTokenList modifiers, Accessibility byDefault)
        => modifiers.Any(SyntaxKind.PublicKeyword) ? Accessibility.Public
        : modifiers.Any(SyntaxKind.ProtectedKeyword) && modifiers.Any(SyntaxKind.InternalKeyword) ? Accessibility.ProtectedOrInternal
        : modifiers.Any(SyntaxKind.ProtectedKeyword) && modifiers.Any(SyntaxKind.PrivateKeyword) ? Accessibility.ProtectedAndInternal
        : modifiers.Any(SyntaxKind.ProtectedKeyword) ? Accessibility.Protected
        : modifiers.Any(SyntaxKind.InternalKeyword) ? Accessibility.Internal
        : modifiers.Any(SyntaxKind.PrivateKeyword) ? Accessibility.Private
        : byDefault;

    private static Accessibility ByDefault(MemberDeclarationSyntax member)
        => member.Parent switch
        {
            InterfaceDeclarationSyntax or EnumDeclarationSyntax => Accessibility.Public,
            BaseNamespaceDeclarationSyntax or CompilationUnitSyntax => Accessibility.Internal,
            _ => Accessibility.Private,
        };
}
