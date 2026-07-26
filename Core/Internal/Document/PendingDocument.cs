namespace TSpec.Internal.Document;

/// <summary>
/// A resolved document, ready to write. Preparing it is everything that can fail;
/// writing it cannot. Separated so the whole resolution chain is testable without a test run.
/// </summary>
internal sealed record PendingDocument(string Path, string Content)
{
    internal static PendingDocument Prepare(string specAssemblyName, string baseDirectory)
    {
        var references = ProjectReferences.Read(baseDirectory, specAssemblyName);
        var subject = SpecificationSubject.Resolve(specAssemblyName, references);
        var directory = ProjectDirectory.Locate(baseDirectory);
        return new(
            System.IO.Path.Combine(directory, SpecificationDocument.FileName),
            DocumentRenderer.Render(subject, specAssemblyName));
    }

    internal void Write() => File.WriteAllText(Path, Content);
}
