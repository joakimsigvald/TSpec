namespace TSpec.Internal.Document;

/// <summary>
/// The project a specification document describes, and its version.
/// The name is derived from the spec assembly name by stripping its last suffix
/// (MyHotel.Spec describes MyHotel), then verified against the build's project references.
/// </summary>
internal sealed record SpecificationSubject(string Name, string Version)
{
    /// <summary>
    /// Stated identically by both failures, so the fix reads the same whichever half of the rule broke.
    /// </summary>
    internal const string _expectations =
        "A spec project must (1) be named after the project it specifies with one suffix appended — "
        + "'MyHotel.Spec' is preferred, 'MyHotel.Test' is fine — and (2) reference that project "
        + "directly; a transitive reference is not enough.";

    internal static SpecificationSubject Resolve(string specAssemblyName, ProjectReferences references)
    {
        var name = DeriveName(specAssemblyName);
        if (!references.TryGetVersion(name, out var version))
            throw new SetupFailed(
                $"TSpec derived the subject '{name}' from the spec assembly name '{specAssemblyName}', "
                + $"but '{name}' is not one of its direct project references ({Describe(references.Names)}). "
                + _expectations);
        return new(name, version);
    }

    internal static string DeriveName(string specAssemblyName)
    {
        var lastSeparator = specAssemblyName.LastIndexOf('.');
        if (lastSeparator <= 0 || lastSeparator == specAssemblyName.Length - 1)
            throw new SetupFailed(
                $"TSpec cannot tell which project the spec assembly '{specAssemblyName}' specifies: "
                + "the name has no suffix to strip. " + _expectations);
        return specAssemblyName[..lastSeparator];
    }

    private static string Describe(IReadOnlyCollection<string> names)
        => names.Count == 0 ? "it has none" : string.Join(", ", names.Order());
}
