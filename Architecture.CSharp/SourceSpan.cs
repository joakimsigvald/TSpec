namespace TSpec.Architecture.CSharp;

/// <summary>
/// Where a piece of a source file starts and ends
/// </summary>
/// <param name="Start">The place of its first character</param>
/// <param name="End">The place just after its last character, as an editor's selection ends</param>
public readonly record struct SourceSpan(SourcePosition Start, SourcePosition End)
{
    /// <summary>
    /// The span as its two places: "12:5-14:2"
    /// </summary>
    public override string ToString() => $"{Start}-{End}";
}
