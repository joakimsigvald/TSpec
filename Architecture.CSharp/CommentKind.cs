namespace TSpec.Architecture.CSharp;

/// <summary>
/// How a comment is written
/// </summary>
public enum CommentKind
{
    /// <summary>
    /// <c>//</c>, on one line or on several in a row
    /// </summary>
    Line,

    /// <summary>
    /// <c>/* */</c>
    /// </summary>
    Block,

    /// <summary>
    /// <c>///</c>, on one line or on several in a row
    /// </summary>
    Documentation,

    /// <summary>
    /// <c>/** */</c>
    /// </summary>
    DocumentationBlock,
}
