using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;

namespace MyHotel.Architecture.Spec;

/// <summary>A source file as the shape facts read it: where its nodes sit, read from the spans TSpec gives,
/// and whether it mirrors a schema.</summary>
internal static class SourceFiles
{
    // A type that mirrors a schema defined elsewhere lives in a Mirrors folder (0e, 10a). The schema dictates
    // its width and its grouping; review judges what else it dictates.
    private const string MirrorsFolder = "/Mirrors/";

    extension(SourceFile file)
    {
        public bool MirrorsASchema => file.ToString().Contains(MirrorsFolder, StringComparison.Ordinal);

        /// <summary>The line a node starts on, counted from 1.</summary>
        public int Line(SyntaxNodeOrToken node) => file.SpanOf(node).Start.Line;

        /// <summary>The line a node ends on, counted from 1 as Line is.</summary>
        public int End(SyntaxNode node) => file.SpanOf(node).End.Line;

        /// <summary>How far into its line a node starts, counted from 0.</summary>
        public int Column(SyntaxNodeOrToken node) => file.SpanOf(node).Start.Column - 1;

        /// <summary>Whether a statement's body sits on its head's line; a block never does.</summary>
        public bool Beside(SyntaxToken head, StatementSyntax body) =>
            body is not BlockSyntax && file.Line(head) == file.Line(body);
    }
}
