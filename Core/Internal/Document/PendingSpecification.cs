using TSpec.Internal.Document.RenderPipeline;

namespace TSpec.Internal.Document;

/// <summary>
/// A located specification, ready to write: one file per top-level folder of the spec project, and
/// one for what sits at its root, in a folder of their own. Everything that can fail is resolved
/// when it is prepared — before any test runs — while the content depends on requirements
/// collected during the run.
/// </summary>
internal sealed record PendingSpecification(
    string Directory, SpecificationSubject Subject, string SpecAssemblyName)
{
    internal static PendingSpecification Prepare(string specAssemblyName, string baseDirectory)
    {
        var references = ProjectReferences.Read(baseDirectory, specAssemblyName);
        var subject = SpecificationSubject.Resolve(specAssemblyName, references);
        subject = subject with { Description = SubjectDescription.Of(subject.Name) };
        var directory = ProjectDirectory.Locate(baseDirectory);
        return new(Path.Combine(directory, SpecificationDocument.FolderName), subject, specAssemblyName);
    }

    /// The spec project's directory, which the documents' links to source are found under.
    internal string SourceRoot => Path.GetDirectoryName(Directory)!;

    internal IReadOnlyList<SpecificationFile> Render(IEnumerable<SpecificationEntry> entries)
        => DocumentRenderer.Render(Subject, SpecAssemblyName, entries, SourceRoot);

    /// <summary>
    /// A full run replaces the folder's contents, so a file from an earlier run — a folder since
    /// renamed or removed — does not stay on stating requirements nothing runs any more.
    /// </summary>
    internal void Write(IEnumerable<SpecificationEntry> entries)
    {
        var files = Render(entries);
        System.IO.Directory.CreateDirectory(Directory);
        var written = files.Select(file => file.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var stale in System.IO.Directory.EnumerateFiles(Directory))
            if (!written.Contains(Path.GetFileName(stale)))
                File.Delete(stale);
        foreach (var file in files)
            File.WriteAllText(Path.Combine(Directory, file.FileName), file.Content);
    }
}
