using TSpec.Internal.Document.RenderPipeline;

namespace TSpec.Internal.Document;

/// <summary>
/// A located document, ready to write. Everything that can fail is resolved when it is prepared —
/// before any test runs — while the content depends on requirements collected during the run.
/// </summary>
internal sealed record PendingDocument(
    string Path, SpecificationSubject Subject, string SpecAssemblyName, string? SourceId)
{
    /// <summary>
    /// The id covers the subject and everything it is built on, which is only known once the
    /// references have been read — so it is taken here rather than handed in.
    /// </summary>
    internal static PendingDocument Prepare(string specAssemblyName, string baseDirectory)
    {
        var references = ProjectReferences.Read(baseDirectory, specAssemblyName);
        var subject = SpecificationSubject.Resolve(specAssemblyName, references);
        var directory = ProjectDirectory.Locate(baseDirectory);
        return new(
            System.IO.Path.Combine(directory, SpecificationDocument.FileName), subject, specAssemblyName,
            SourceDigest.Of(Specified(references, subject, baseDirectory)));
    }

    private static IEnumerable<string> Specified(
        ProjectReferences references, SpecificationSubject subject, string baseDirectory)
        => references.ClosureFrom(subject.Name)
            .Select(name => System.IO.Path.Combine(baseDirectory, $"{name}.dll"));

    internal string Render(IEnumerable<SpecificationEntry> entries)
        => DocumentRenderer.Render(Subject, SpecAssemblyName, SourceId, entries);

    internal void Write(IEnumerable<SpecificationEntry> entries)
        => File.WriteAllText(Path, Render(entries));
}
