namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record TitleSegment(string Name) : DocumentSegment
{
    internal override string Render() => $"# {Name}\n";
}
