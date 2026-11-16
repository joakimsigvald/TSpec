using System.Diagnostics.CodeAnalysis;
using TSpec.Architecture;
using TSpec.Extensibility;

#pragma warning disable CS0618 // The architecture page reads TSpec's deprecated project graph until it moves to the package too.

namespace TSpec.Internal.Document;

internal static class ProjectFolder
{
    internal static bool TryFind(SourceProject project, [NotNullWhen(true)] out string? folder)
    {
        folder = null;
        if (!Build.TryOpenDebugInformation(project, out var provider))
            return false;

        using (provider)
        {
            var debug = provider.GetMetadataReader();
            return Build.TryFindProjectFolder(
                [.. debug.Documents.Select(document => debug.GetString(debug.GetDocument(document).Name))], out folder);
        }
    }
}
