using TSpec.Internal.Document.RenderPipeline;

namespace TSpec.Internal.Document;

/// <summary>
/// A located document, ready to write. Everything that can fail is resolved when it is prepared —
/// before any test runs — while the content depends on requirements collected during the run.
/// </summary>
internal sealed record PendingDocument(
    string Path, SpecificationSubject Subject, string SpecAssemblyName, string BuildId)
{
    internal static PendingDocument Prepare(string specAssemblyName, string buildId, string baseDirectory)
    {
        var references = ProjectReferences.Read(baseDirectory, specAssemblyName);
        var subject = SpecificationSubject.Resolve(specAssemblyName, references);
        var directory = ProjectDirectory.Locate(baseDirectory);
        return new(
            System.IO.Path.Combine(directory, SpecificationDocument.FileName), subject, specAssemblyName, buildId);
    }

    internal string Render(IEnumerable<SpecificationEntry> entries)
        => DocumentRenderer.Render(Subject, SpecAssemblyName, BuildId, entries);

    internal void Write(IEnumerable<SpecificationEntry> entries)
        => File.WriteAllText(Path, Render(entries));
}
