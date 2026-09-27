using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TSpec.Generator;

internal static class ShareableThen
{
    private static readonly HashSet<SpecialType> _builtInTypes =
    [
        SpecialType.System_Boolean, SpecialType.System_Char, SpecialType.System_String,
        SpecialType.System_SByte, SpecialType.System_Byte, SpecialType.System_Int16, SpecialType.System_UInt16,
        SpecialType.System_Int32, SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64,
        SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal,
    ];

    internal static bool Is(IMethodSymbol method, MethodDeclarationSyntax declaration, SemanticModel model)
        => method.ReturnsVoid
            && !method.IsAsync
            && CanBeNamed(method.ContainingType)
            && Statements(declaration) is { Length: > 0 } statements
            && statements.All(statement => AssertsOnResult(statement, model));

    private static ExpressionSyntax[] Statements(MethodDeclarationSyntax declaration)
        => declaration.ExpressionBody is { } arrow ? [arrow.Expression]
            : declaration.Body is { } block && block.Statements.All(it => it is ExpressionStatementSyntax)
                ? [.. block.Statements.Cast<ExpressionStatementSyntax>().Select(it => it.Expression)]
            : [];

    private static bool AssertsOnResult(ExpressionSyntax expression, SemanticModel model)
        => expression switch
        {
            InvocationExpressionSyntax call => HasConstantArguments(call, model) && AssertsOnResult(call.Expression, model),
            MemberAccessExpressionSyntax access => IsFromTSpec(model.GetSymbolInfo(access).Symbol)
                && AssertsOnResult(access.Expression, model),
            IdentifierNameSyntax name => IsResult(model.GetSymbolInfo(name).Symbol)
                && IsBuiltIn(model.GetTypeInfo(name).Type),
            _ => false,
        };

    private static bool HasConstantArguments(InvocationExpressionSyntax call, SemanticModel model)
        => call.ArgumentList.Arguments.All(it => model.GetConstantValue(it.Expression).HasValue);

    private static bool IsResult(ISymbol? symbol)
        => symbol is IPropertySymbol { Name: "Result" } && IsFromTSpec(symbol);

    private static bool IsFromTSpec(ISymbol? symbol) => symbol?.ContainingAssembly?.Name == "TSpec";

    private static bool IsBuiltIn(ITypeSymbol? type)
        => type switch
        {
            INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
                => IsBuiltIn(nullable.TypeArguments[0]),
            { TypeKind: TypeKind.Enum } => true,
            { } other => _builtInTypes.Contains(other.SpecialType),
            null => false,
        };

    /// The typeof in an assembly attribute reaches only a type visible to the whole assembly, and no open generic.
    private static bool CanBeNamed(INamedTypeSymbol? type)
        => type is null
            || (type.TypeParameters.IsEmpty
                && type.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal
                    or Accessibility.ProtectedOrInternal
                && CanBeNamed(type.ContainingType));
}
