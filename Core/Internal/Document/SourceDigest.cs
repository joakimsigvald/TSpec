using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;

namespace TSpec.Internal.Document;

/// <summary>One source file the compiler read, and the checksum it recorded for its contents.</summary>
internal readonly record struct SourceFile(string Path, string Checksum);

/// <summary>
/// Identifies the source an assembly was compiled from, by combining the per-file checksums the
/// compiler wrote into its debug information.
/// </summary>
internal static class SourceDigest
{
    /// <summary>The digest of an assembly's source, or null when it carries no debug information.</summary>
    internal static string? Of(string assemblyPath) => Of([assemblyPath]);

    /// <summary>
    /// The digest of the source of several assemblies together, which is one digest over all their
    /// files rather than a digest of digests — so it does not depend on the order they arrive in,
    /// and an assembly the build did not produce simply contributes nothing.
    /// </summary>
    internal static string? Of(IEnumerable<string> assemblyPaths)
    {
        var authored = Authored(assemblyPaths.SelectMany(Read));
        return authored.Count == 0 ? null : Combine(authored);
    }

    internal static string Of(IEnumerable<SourceFile> files) => Combine(Authored(files));

    /// <summary>
    /// The files a developer wrote. The rest are the build's own — they live under the
    /// intermediate output directory, and one of them, the generated assembly info, carries the
    /// commit the build ran at. Counting that one moved the digest on every commit, which is the
    /// opposite of identifying source.
    /// </summary>
    private static IReadOnlyList<SourceFile> Authored(IEnumerable<SourceFile> files)
        => [.. files.Where(file => !IsGenerated(file.Path))];

    private static bool IsGenerated(string path)
        => path.Split('/', '\\').Any(segment => segment.Equals("obj", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Ordered by checksum rather than by path, because the paths are the ones the compiler saw
    /// and would tie the digest to the directory the build ran in.
    /// </summary>
    private static string Combine(IEnumerable<SourceFile> files)
    {
        var checksums = files.Select(file => file.Checksum).OrderBy(checksum => checksum, StringComparer.Ordinal);
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', checksums)));
        return Convert.ToHexString(digest, 0, 4).ToLowerInvariant();
    }

    /// <summary>
    /// The source files recorded in the assembly's portable PDB, embedded or alongside. Anything
    /// that stops it being readable — no debug information, a native PDB, a file being written —
    /// yields no files rather than a failure, since the document is worth writing without an id.
    /// </summary>
    internal static IReadOnlyList<SourceFile> Read(string assemblyPath)
    {
        if (!File.Exists(assemblyPath))
            return [];
        try
        {
            using var stream = File.OpenRead(assemblyPath);
            using var assembly = new PEReader(stream);
            using var pdb = OpenPdb(assembly, assemblyPath);
            return pdb is null ? [] : ReadDocuments(pdb.GetMetadataReader());
        }
        catch (Exception exception) when (exception is BadImageFormatException or IOException)
        {
            return [];
        }
    }

    private static MetadataReaderProvider? OpenPdb(PEReader assembly, string assemblyPath)
    {
        foreach (var entry in assembly.ReadDebugDirectory())
            if (entry.Type == DebugDirectoryEntryType.EmbeddedPortablePdb)
                return assembly.ReadEmbeddedPortablePdbDebugDirectoryData(entry);
        var path = Path.ChangeExtension(assemblyPath, ".pdb");
        return File.Exists(path)
            ? MetadataReaderProvider.FromPortablePdbStream(new MemoryStream(File.ReadAllBytes(path)))
            : null;
    }

    private static List<SourceFile> ReadDocuments(MetadataReader pdb)
    {
        var files = new List<SourceFile>(pdb.Documents.Count);
        foreach (var handle in pdb.Documents)
        {
            var document = pdb.GetDocument(handle);
            files.Add(new(
                pdb.GetString(document.Name), Convert.ToHexString(pdb.GetBlobBytes(document.Hash))));
        }
        return files;
    }
}
