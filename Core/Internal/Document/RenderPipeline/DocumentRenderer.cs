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
        Represent represent = new(subject, specAssemblyName, buildId, entries);
        Layout layout = new(represent.Document);
        Render render = new(layout.Segments);
        return render.ToString();
    }
}