using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// The README of the specification folder: the project's title, what its project file says of it,
/// and what holds throughout it, above a table with one row per file saying what it covers and how
/// much. It is the entry point the folder's files share — and in a review, its diff is one line
/// per file saying where a change landed.
/// </summary>
internal static class IndexRenderer
{
    internal const string Name = "README";

    internal static string Render(
        SpecificationSubject subject, string specAssemblyName,
        IReadOnlyList<Requirement> all, IReadOnlyList<Document> documents)
    {
        List<DocumentSegment> segments = [
            new TitleSegment(subject.Name.AsTitle()),
            CommentSegment.Generated(subject.Version, specAssemblyName)];
        if (subject.Description is not null)
            segments.Add(new ParagraphSegment(subject.Description));
        segments.Add(new CodeSegment(
            Requirement.SubjectOf(all), Requirement.ReturnTypeOf(all),
            Requirement.Shared(all, acts: false), Stated: null));
        segments.Add(new IndexTableSegment(documents));
        return segments.ToArray().Render();
    }
}
