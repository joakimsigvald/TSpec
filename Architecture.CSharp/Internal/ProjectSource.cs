using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// What the compiler read for a project: its files, and the documents the build generated or linked in.
/// </summary>
internal sealed record ProjectSource(IReadOnlyList<SourceFile> Files, IReadOnlyList<string> Others, CSharpParseOptions Options)
{
    // A document the compiler read from memory, such as a source generator's, is not on disk to read again.
    internal IEnumerable<SyntaxTree> Trees
        => Files.Select(file => file.Root.SyntaxTree).Concat(Others.Where(File.Exists).Select(Parsed));

    private SyntaxTree Parsed(string document) => CSharpSyntaxTree.ParseText(File.ReadAllText(document), Options, document);
}
