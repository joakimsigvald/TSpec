using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The size of a run of a file's lines, each weighed by its indentation level.
/// </summary>
internal static class Sizes
{
    internal static double Of(SourceFile file, SourceSpan span)
    {
        var start = file.Levels[span.Start.Line - 1];
        return Enumerable.Range(span.Start.Line - 1, span.Lines).Sum(index => WeightOf(file.Levels[index], start));
    }

    // The compiler keeps top-level statements before the file's types, so they run from the first to the last.
    internal static double OfTopLevel(SourceFile file)
        => file.Root.Members.OfType<GlobalStatementSyntax>().ToArray() is [var first, ..] and var statements
            ? Of(file, new(file.SpanOf(first).Start, file.SpanOf(statements[^1]).End))
            : 0;

    // Each level the run stands at costs the square root of 2, and each level inside it 2.
    private static double WeightOf(int level, int start) => Math.Pow(2, level - start / 2.0);
}
