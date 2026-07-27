using TSpec.Internal.Specification;

namespace TSpec.Internal.Document;

/// <summary>
/// One passing requirement, as it will appear in the document.
/// </summary>
/// <param name="Subject">The outermost test class — the method under test, e.g. WhenGetVersion.</param>
/// <param name="Branch">The nested given-classes leading to the test, dotted; empty when there are none.</param>
/// <param name="Requirement">The test method name.</param>
/// <param name="Steps">The described steps, not yet laid out. The document renders them itself,
/// which is what will let it arrange them differently than a single test does.</param>
/// <param name="Because">The reason given for the requirement, if any.</param>
internal sealed record SpecificationEntry(
    string Subject,
    string Branch,
    string Requirement,
    IReadOnlyList<SpecificationStep> Steps,
    string? Because = null);
