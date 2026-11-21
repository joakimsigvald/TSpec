using System.Text;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// Renders the specification's files from test-specification: one per top-level folder of the spec
/// project, and one for what sits at its root.
/// Line endings normalized to LF for platform independence.
/// Text normalized and organized to make equivalent specs byte-identical on every machine, allowing for automated diff-detection.
/// </summary>
internal static class DocumentRenderer
{
    private const int FolderLevel = 2;
    /// The README first, being the one to open first; then the folders' files, by name.
    internal static IReadOnlyList<SpecificationFile> Render(
        SpecificationSubject subject, string specAssemblyName,
        IEnumerable<SpecificationEntry> entries, string? sourceRoot = null)
    {
        Requirement[] requirements = [.. Requirement.From(entries)];
        var documents = Document.Of(subject.Name, specAssemblyName, requirements, sourceRoot, linkBase: "../");
        if (documents.Count == 0)
            return [];
        return [
            new(IndexRenderer.Name, IndexRenderer.Render(subject, specAssemblyName, requirements, documents)),
            .. documents.Select(document => new SpecificationFile(document.Name, document.Layout(subject.Version).Render()))];
    }

    /// <summary>
    /// The documents as sections of one page that sits in the spec project's folder and is headed by its
    /// own title: what sits at the project's root first, then each folder under a heading naming it,
    /// its own headings a level below.
    /// </summary>
    internal static IEnumerable<DocumentSegment> Sections(
        string subjectName, string specAssemblyName, IEnumerable<SpecificationEntry> entries, string? sourceRoot)
        => Document.Of(subjectName, specAssemblyName, [.. Requirement.From(entries)], sourceRoot, linkBase: "")
            .OrderBy(document => document.Root.HasKey)
            .SelectMany(Section);

    private static DocumentSegment[] Section(Document document)
        => document.Root.HasKey
            ? [new HeadingSegment(document.Title, FolderLevel), .. document.Body(shift: 1)]
            : document.Body(shift: 0);

    private static DocumentSegment[] Layout(this Document document, string version)
        => [new TitleSegment(document.Title),
            CommentSegment.Generated(version, document.GeneratedFrom),
            .. document.Body(shift: 0)];

    private static DocumentSegment[] Body(this Document _document, int shift)
    {
        List<DocumentSegment> _segments = [];
        var root = _document.Root;
        Add(new CodeSegment(root.SubjectUnderTest, root.ReturnType, root.Shared, Stated: null));
        foreach (var section in Order(root.Children))
            AddNode(section, root.SubjectUnderTest, root.ReturnType);
        return [.. _segments];

        void Add(DocumentSegment segment) => _segments.Add(segment);

        void AddNode(DocumentNode node, string? statedSubject, string? statedReturnType)
        {
            if (node.Heading is not null)
                AddHeading(node, node.Heading, statedSubject, statedReturnType);
            if (node.Requirements.Count > 0)
                AddLeaves(node.Requirements);
            foreach (var section in Order(node.Children))
                AddNode(section,
                    statedSubject ?? node.SubjectUnderTest,
                    statedReturnType ?? node.ReturnType);
        }

        void AddHeading(DocumentNode node, string heading, string? statedSubject, string? statedReturnType)
        {
            Add(new HeadingSegment(heading, node.Level + shift, _document.Href(node.Source)));
            Add(new CodeSegment(
                statedSubject is null ? node.SubjectUnderTest : null,
                statedReturnType is null ? node.ReturnType : null,
                node.Shared,
                StatedWord(heading)));
        }

        void AddLeaves(IReadOnlyList<Requirement> requirements)
        {
            Add(new NewLineSegment());
            foreach (var requirement in InReadingOrder(requirements))
                Add(new ListItemSegment(requirement.Name, requirement.Claim, requirement.Table));
        }
    }

    /// As declared, which is the order they run in.
    private static IEnumerable<Requirement> InReadingOrder(IReadOnlyList<Requirement> requirements)
        => requirements
            .OrderBy(requirement => requirement.Entry.Declared)
            .ThenBy(requirement => requirement.Entry.Requirement, StringComparer.Ordinal)
            .ThenBy(requirement => requirement.Signature, StringComparer.Ordinal);

    /// By what a change to a section leaves alone, so a reviewed diff shows the change in place.
    private static DocumentNode[] Order(IEnumerable<DocumentNode> nodes)
        => [..nodes.OrderBy(node => node.HasKey)
            .ThenBy(node => node.Declared, Comparer<IReadOnlyList<int>>.Create(CompareDeclared))
            .ThenBy(node => node.Key, StringComparer.Ordinal)];

    /// One class of the path at a time, outermost first, since a class is numbered only among its siblings.
    private static int CompareDeclared(IReadOnlyList<int> one, IReadOnlyList<int> other)
    {
        var differs = one.Zip(other, (a, b) => a.CompareTo(b)).FirstOrDefault(order => order != 0);
        return differs != 0 ? differs : one.Count.CompareTo(other.Count);
    }

    /// <summary>
    /// The subject's version says which release the document describes; the id says which source
    /// that release was built from. A subject compiled without debug information has no source to
    /// name, and the version stands alone rather than trailing an empty suffix.
    /// </summary>
    internal static string? StatedWord(string heading)
        => StepFamilies.Keywords.FirstOrDefault(
            word => heading.StartsWith($"{word} ", StringComparison.Ordinal));

    internal static string Render(this DocumentSegment[] segments)
    {
        StringBuilder text = new();
        foreach (var segment in segments)
            text.Append(segment.Render());
        return Squeezed(text.ToString());
    }

    /// <summary>
    /// One blank line at most. Segments cannot see each other, so a table that closes its item with
    /// a blank line and a heading that opens with one would together leave a gap the document
    /// leaves nowhere else.
    /// </summary>
    private static string Squeezed(string text)
        => text.Contains("\n\n\n", StringComparison.Ordinal)
            ? Squeezed(text.Replace("\n\n\n", "\n\n", StringComparison.Ordinal))
            : text;
}