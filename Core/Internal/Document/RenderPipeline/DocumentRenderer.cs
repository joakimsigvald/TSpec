using System.Text;

namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// Renders a specification document from test-specification. 
/// Line endings normalized to LF for platform independence.
/// Text normalized and organized to make equivalent specs byte-identical on every machine, allowing for automated diff-detection.
/// </summary>
internal class DocumentRenderer
{
    internal static string Render(
        SpecificationSubject subject, string specAssemblyName, string buildId,
        IEnumerable<SpecificationEntry> entries)
    {
        var doc = Document.Of(subject, specAssemblyName, buildId, entries);
        Layout layout = new(doc);
        return Render(layout.Segments);
    }

    private static string Render(DocumentSegment[] segments)
    {
        StringBuilder text = new();
        foreach (var segment in segments)
            text.Append(segment.Render());
        return text.ToString();
    }
}