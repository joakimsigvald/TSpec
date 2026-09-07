namespace TSpec.Internal.Document;

/// <summary>
/// One rendered file of the specification: named for the folder it describes, for the project
/// where it describes what sits at the root, or README for the index of the others.
/// </summary>
internal sealed record SpecificationFile(string Name, string Content)
{
    internal string FileName => FileNameOf(Name);

    internal static string FileNameOf(string name) => $"{name}.md";
}
