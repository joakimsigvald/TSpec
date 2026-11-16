using System.Diagnostics.CodeAnalysis;
using TSpec.Extensibility;

namespace TSpec.Internal.Document;

internal static class ProjectFolder
{
    internal static bool TryFind(string assemblyName, [NotNullWhen(true)] out string? folder)
    {
        folder = null;
        if (!Build.TryOpenDebugInformation(assemblyName, out var provider))
            return false;

        using (provider)
        {
            var debug = provider.GetMetadataReader();
            return Build.TryFindProjectFolder(
                [.. debug.Documents.Select(document => debug.GetString(debug.GetDocument(document).Name))], out folder);
        }
    }
}
