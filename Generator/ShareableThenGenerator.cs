using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TSpec.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class ShareableThenGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
        => context.RegisterSourceOutput(ShareableThens(context).Collect(), Emit);

    private static IncrementalValuesProvider<(string SpecClass, string Method)> ShareableThens(
        IncrementalGeneratorInitializationContext context)
        => context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Xunit.FactAttribute",
                static (node, _) => node is MethodDeclarationSyntax,
                static (context, _) => Recognise(context))
            .Where(static then => then.HasValue)
            .Select(static (then, _) => then!.Value);

    private static (string SpecClass, string Method)? Recognise(GeneratorAttributeSyntaxContext context)
        => context.TargetSymbol is IMethodSymbol method
            && ShareableThen.Is(method, (MethodDeclarationSyntax)context.TargetNode, context.SemanticModel)
            ? (method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), method.Name)
            : null;

    private static void Emit(SourceProductionContext context, ImmutableArray<(string SpecClass, string Method)> thens)
    {
        if (thens.IsEmpty)
            return;

        context.AddSource("ShareableThens.g.cs", string.Join("\n", thens.Select(Attribute)));
    }

    private static string Attribute((string SpecClass, string Method) then)
        => $"[assembly: global::TSpec.ShareableThen(typeof({then.SpecClass}), \"{then.Method}\")]";
}
