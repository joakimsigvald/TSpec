namespace TSpec.Internal.Document;

/// <summary>
/// One rendered file of the specification: named for the folder it describes, or for the project
/// where it describes what sits at the root.
/// </summary>
internal sealed record SpecificationFile(string Name, string Content)
{
    internal string FileName => $"{Name}.md";
}
