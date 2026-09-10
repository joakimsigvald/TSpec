using TSpec.Internal.Document.RenderPipeline;
using TSpec.Internal.Specification;

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
    /// <summary>
    /// None where the spec project's directory cannot be located, which is not a claim any test
    /// makes and so not a reason to fail one. A project that names or references its subject
    /// wrongly still fails here: that is a rule about the spec project itself. The spec classes'
    /// own source files say which project directory to write into.
    /// </summary>
    internal static PendingSpecification? Prepare(
        string specAssemblyName, string baseDirectory, IReadOnlyCollection<string>? specSources = null)
    {
        var references = ProjectReferences.Read(baseDirectory, specAssemblyName);
        var subject = SpecificationSubject.Resolve(specAssemblyName, references);
        subject = subject with { Description = SubjectDescription.Of(subject.Name) };
        return ProjectDirectory.TryLocate(specSources ?? [], baseDirectory, out var directory)
            ? new(Path.Combine(directory, SpecificationDocument.FolderName), subject, specAssemblyName)
            : null;
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
            Write(Path.Combine(Directory, file.FileName), file.Content);
    }

    /// <summary>
    /// Keeping the line endings the file already has, so a checkout that holds them as carriage
    /// returns does not report every regenerated file as modified.
    /// </summary>
    private static void Write(string path, string content)
        => File.WriteAllText(
            path, File.Exists(path) ? content.WithLineEndingsOf(File.ReadAllText(path)) : content);
}
