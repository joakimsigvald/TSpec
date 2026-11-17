using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MyHotel.Architecture.Spec;

/// <summary>A snippet of C# compiled against the runtime the spec runs on, so a fact's examples are bound as
/// the build's code is: a name resolves to what it stands for, an alias too.</summary>
internal static class Snippets
{
    private static readonly Lazy<IReadOnlyList<MetadataReference>> Runtime = new(RuntimeAssemblies);

    /// <summary>The model of a snippet; a name the snippet neither declares nor imports stays
    /// unresolved.</summary>
    public static SemanticModel Compiled(string code) => ModelOf(CSharpSyntaxTree.ParseText(code));

    public static IReadOnlyList<T> Nodes<T>(SemanticModel snippet) where T : SyntaxNode =>
        [.. snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<T>()];

    private static SemanticModel ModelOf(SyntaxTree tree) =>
        CSharpCompilation.Create("Snippet", [tree], Runtime.Value).GetSemanticModel(tree);

    private static IReadOnlyList<MetadataReference> RuntimeAssemblies() =>
        [.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))];
}
