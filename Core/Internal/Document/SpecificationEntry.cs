namespace TSpec.Internal.Document;

/// <summary>
/// One passing requirement, as it will appear in the document.
/// </summary>
/// <param name="Subject">The outermost test class — the method under test, e.g. WhenGetVersion.</param>
/// <param name="Branch">The nested given-classes leading to the test, dotted; empty when there are none.</param>
/// <param name="Requirement">The test method name.</param>
/// <param name="Text">The rendered specification, exactly as a failing test would report it.</param>
internal sealed record SpecificationEntry(string Subject, string Branch, string Requirement, string Text);
