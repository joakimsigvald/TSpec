using TSpec.Internal.Specification;

namespace TSpec.Internal.Document;

/// <summary>
/// One passing requirement, as it will appear in the document.
/// </summary>
/// <param name="Subject">The outermost test class — the method under test, e.g. WhenGetVersion.</param>
/// <param name="Branch">The nested given-classes leading to the test, dotted; empty when there are none.</param>
/// <param name="Requirement">The test method name.</param>
/// <param name="Clauses">The described clauses, not yet laid out. The document renders them itself,
/// which is what will let it arrange them differently than a single test does.</param>
/// <param name="Because">The reason given for the requirement, if any.</param>
/// <param name="SubjectUnderTest">The type the spec class drives, e.g. HttpClient; null when it
/// declares none. Named in full because <c>Subject</c> already means the heading a requirement
/// sits under.</param>
/// <param name="ReturnType">The return type of the method under test; null with the above.</param>
/// <param name="Namespace">The namespace of the test class, whole. What the document heads a section
/// with is the segment the namespaces differ in, which only the whole set of them can tell.</param>
internal sealed record SpecificationEntry(
    string Subject,
    string Branch,
    string Requirement,
    IReadOnlyList<SpecificationClause> Clauses,
    string? Because = null,
    string? SubjectUnderTest = null,
    string? ReturnType = null,
    string? Namespace = null);
