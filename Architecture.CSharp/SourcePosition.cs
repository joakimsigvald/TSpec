namespace TSpec.Architecture.CSharp;

/// <summary>
/// A place in a source file, counted from 1 as an editor counts it
/// </summary>
/// <param name="Line">The line, counted from 1</param>
/// <param name="Column">The column, counted from 1</param>
public readonly record struct SourcePosition(int Line, int Column)
{
    /// <summary>
    /// The place as an editor writes it: "12:5"
    /// </summary>
    public override string ToString() => $"{Line}:{Column}";
}
