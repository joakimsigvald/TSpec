namespace TSpec.Internal.Document.RenderPipeline;

internal class Represent(
        SpecificationSubject subject,
        string specAssemblyName,
        string buildId,
        IEnumerable<SpecificationEntry> entries)
{
    private Document? _document;

    internal Document Document => _document ??= Document.Of(subject, specAssemblyName, buildId, entries);
}
