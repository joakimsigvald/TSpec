using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace TSpec.Internal.Document;

/// <summary>
/// Reads where a test class or method is written from the portable debug information the build
/// left beside its assembly, or embedded in it. A method is where its first statement is; a class
/// is where its constructor is, or failing that in the file its members are written in.
/// </summary>
internal static class SourceLocations
{
    private const BindingFlags Own = BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static readonly ConcurrentDictionary<Module, DebugInformation?> _byModule = new();

    /// An async or iterator method keeps its lines in the state machine it compiles into.
    internal static SourceLocation? Of(MethodBase method)
        => _byModule.GetOrAdd(method.Module, Open)?.Locate(
            method.GetCustomAttribute<StateMachineAttribute>()?.StateMachineType
                .GetMethod(nameof(IAsyncStateMachine.MoveNext), Own) ?? method);

    internal static SourceLocation? Of(Type type)
    {
        var constructed = type.GetConstructors(Own).Select(Of).FirstOrDefault(at => at?.Line is not null);
        if (constructed is not null)
            return constructed;
        var file = type.GetMethods(Own).Select(Of).FirstOrDefault(at => at is not null)?.File
            ?? (type.DeclaringType is { } outer ? Of(outer)?.File : null);
        return file is null ? null : new(file, null);
    }

    private static DebugInformation? Open(Module module)
    {
        var path = module.Assembly.Location;
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return null;
        using var image = new PEReader(File.OpenRead(path));
        return image.TryOpenAssociatedPortablePdb(path, File.OpenRead, out var provider, out _)
            ? new(provider!)
            : null;
    }

    private sealed class DebugInformation(MetadataReaderProvider provider)
    {
        private readonly MetadataReader _reader = provider.GetMetadataReader();

        internal SourceLocation? Locate(MethodBase method)
        {
            var row = MetadataTokens.GetRowNumber(MetadataTokens.EntityHandle(method.MetadataToken));
            var debugInformation = _reader.GetMethodDebugInformation(
                MetadataTokens.MethodDebugInformationHandle(row));
            foreach (var point in debugInformation.GetSequencePoints())
                if (!point.IsHidden)
                    return new(_reader.GetString(_reader.GetDocument(point.Document).Name), point.StartLine);
            return null;
        }
    }
}
