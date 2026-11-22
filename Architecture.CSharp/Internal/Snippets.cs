using Microsoft.CodeAnalysis.CSharp;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// A piece of code read as a source file of its own, compiled against the assemblies the running spec
/// project runs with, as the build's files are.
/// </summary>
internal static class Snippets
{
    internal static SourceFile Of(string code)
    {
        IReadOnlyList<SourceFile> files =
            [new("Snippet", "Snippet.cs", "", () => code, CSharpParseOptions.Default, CompiledProject.ModelOfSnippet)];
        ProjectSources.Declare(TypeDeclarations.In(files), files);
        return files[0];
    }
}
