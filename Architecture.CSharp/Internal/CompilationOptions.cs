using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.CSharp;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// How the build parsed a project, as the compiler records it in the debug information: pairs of
/// null-terminated names and values, such as language-version and define.
/// </summary>
internal static class CompilationOptions
{
    private static readonly Guid _compilationOptions = new("B5FEEC05-8CD0-4A83-96DA-466284BB4BD8");

    internal static CSharpParseOptions ParseOptionsOf(string project, MetadataReader debug)
        => ParseOptions(project, Read(debug));

    internal static CSharpParseOptions ParseOptions(string project, IReadOnlyDictionary<string, string> options)
        => new(languageVersion: LanguageVersionOf(project, options), preprocessorSymbols: SymbolsOf(options));

    private static LanguageVersion LanguageVersionOf(string project, IReadOnlyDictionary<string, string> options) => 
        !options.TryGetValue("language-version", out var recorded) ? LanguageVersion.Default
        : LanguageVersionFacts.TryParse(recorded, out var version) ? version
        : throw new SetupFailed(
            $"TSpec could not read the types of '{project}', written in C# {recorded}: "
            + "reference a newer Microsoft.CodeAnalysis.CSharp in the spec project.");

    private static string[] SymbolsOf(IReadOnlyDictionary<string, string> options)
        => options.TryGetValue("define", out var symbols) ? symbols.Split(',', StringSplitOptions.RemoveEmptyEntries) : [];

    private static Dictionary<string, string> Read(MetadataReader debug)
    {
        Dictionary<string, string> options = [];
        foreach (var handle in debug.GetCustomDebugInformation(EntityHandle.ModuleDefinition))
        {
            var information = debug.GetCustomDebugInformation(handle);
            if (debug.GetGuid(information.Kind) == _compilationOptions)
                AddPairs(debug.GetBlobReader(information.Value), options);
        }
        return options;
    }

    private static void AddPairs(BlobReader blob, Dictionary<string, string> options)
    {
        while (blob.RemainingBytes > 0)
        {
            var name = NullTerminated(ref blob);
            options[name] = NullTerminated(ref blob);
        }
    }

    private static string NullTerminated(ref BlobReader blob)
    {
        var text = blob.ReadUTF8(blob.IndexOf(0));
        blob.ReadByte();
        return text;
    }
}
