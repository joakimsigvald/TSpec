using TSpec.Internal.Document;
using TSpec.Internal.Document.RenderPipeline;

namespace TSpec.Test.Internal.Document;

internal static class SpecificationFiles
{
    /// The folder's files beside the README, which is what every test not about the index reads.
    internal static IEnumerable<SpecificationFile> Documents(this IReadOnlyList<SpecificationFile> files)
        => files.Where(file => file.Name != IndexRenderer.Name);
}
