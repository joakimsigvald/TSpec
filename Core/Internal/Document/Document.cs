using TSpec.Internal.Specification;

namespace TSpec.Internal.Document;

/// <summary>
/// What the document is made of, settled before a character of it is written. The renderer reads
/// this and nothing else, and never writes back to it.
/// </summary>
/// <remarks>
/// It grows toward carrying every decision that can be made without measuring rendered text —
/// what heads a section, what it declares, what order things run in. Choices that depend on how
/// wide something turns out to be stay with the renderer, since width is the renderer's variable.
/// That is also the rule for what may be stored here: composed text, never rendered text.
/// <para>
/// <c>Whole</c> is what every requirement in the document states, which the document says once of
/// itself. The act is left out of it: nothing above a subject is named after the act.
/// </para>
/// </remarks>
internal sealed record Document(
    SpecificationSubject Subject,
    string SpecAssemblyName,
    string BuildId,
    Requirement[] Requirements,
    Declared Declared,
    IReadOnlyList<SpecificationClause> Whole)
{
    internal static Document Of(
        SpecificationSubject subject, string specAssemblyName, string buildId,
        IEnumerable<SpecificationEntry> entries)
    {
        Requirement[] requirements = [.. Requirement.From(entries)];
        return new(subject, specAssemblyName, buildId, requirements,
            Declared.Of(requirements, returns: false),
            Requirement.Shared(requirements, acts: false));
    }
}
