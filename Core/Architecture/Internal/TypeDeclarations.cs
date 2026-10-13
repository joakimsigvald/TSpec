using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace TSpec.Architecture.Internal;

/// <summary>
/// The types an assembly declares at its top level, each with the documents it is written in: those its
/// methods' lines are in, its nested types' included, and those the debug information records for a part
/// without methods, such as an interface or an enum.
/// </summary>
internal sealed partial class TypeDeclarations(MetadataReader metadata, MetadataReader debug)
{
    private static readonly Guid _typeDefinitionDocuments = new("932E74BC-DBA9-4478-8D46-0F32A7BAB3D3");

    internal IEnumerable<TypeDeclaration> All
        => DocumentsByType()
            .Select(entry => (Type: metadata.GetTypeDefinition(entry.Key), Documents: entry.Value))
            .Where(entry => !IsCompilerGenerated(entry.Type))
            .Select(entry => Declaration(entry.Type, entry.Documents));

    private TypeDeclaration Declaration(TypeDefinition type, IEnumerable<DocumentHandle> documents)
        => new(
            NameOf(type),
            TypeParametersOf(type),
            metadata.GetString(type.Namespace),
            KindOf(type),
            (type.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public,
            [.. documents.Select(document => debug.GetString(debug.GetDocument(document).Name))]);

    private Dictionary<TypeDefinitionHandle, HashSet<DocumentHandle>> DocumentsByType()
    {
        Dictionary<TypeDefinitionHandle, HashSet<DocumentHandle>> documents = [];
        foreach (var (type, document) in MethodDocuments().Concat(TypeDocuments()))
        {
            var owner = TopLevelOf(type);
            if (!documents.TryGetValue(owner, out var ofOwner))
                documents[owner] = ofOwner = [];
            ofOwner.Add(document);
        }
        return documents;
    }

    private IEnumerable<(TypeDefinitionHandle, DocumentHandle)> MethodDocuments()
        => metadata.MethodDefinitions.SelectMany(method => debug
            .GetMethodDebugInformation(method.ToDebugInformationHandle())
            .GetSequencePoints()
            .Select(point => (metadata.GetMethodDefinition(method).GetDeclaringType(), point.Document)));

    private IEnumerable<(TypeDefinitionHandle, DocumentHandle)> TypeDocuments()
        => debug.CustomDebugInformation
            .Select(debug.GetCustomDebugInformation)
            .Where(information => debug.GetGuid(information.Kind) == _typeDefinitionDocuments)
            .SelectMany(information => DocumentsIn(debug.GetBlobReader(information.Value))
                .Select(document => ((TypeDefinitionHandle)information.Parent, document)));

    private static IEnumerable<DocumentHandle> DocumentsIn(BlobReader blob)
    {
        while (blob.RemainingBytes > 0)
            yield return MetadataTokens.DocumentHandle(blob.ReadCompressedInteger());
    }

    private TypeDefinitionHandle TopLevelOf(TypeDefinitionHandle type)
    {
        var owner = metadata.GetTypeDefinition(type).GetDeclaringType();
        return owner.IsNil ? type : TopLevelOf(owner);
    }

    // A file-local type's name is mangled too, but it is declared in its file like any other.
    private bool IsCompilerGenerated(TypeDefinition type)
    {
        var name = metadata.GetString(type.Name);
        return name.StartsWith('<') && !FileLocal().IsMatch(name);
    }

    private string NameOf(TypeDefinition type) => WithoutArity(Unmangled(metadata.GetString(type.Name)));

    private IReadOnlyList<string> TypeParametersOf(TypeDefinition type)
        => [.. type.GetGenericParameters().Select(parameter => metadata.GetString(metadata.GetGenericParameter(parameter).Name))];

    private static string Unmangled(string name)
    {
        var local = FileLocal().Match(name);
        return local.Success ? local.Groups[1].Value : name;
    }

    private static string WithoutArity(string name)
    {
        var tick = name.IndexOf('`');
        return tick < 0 ? name : name[..tick];
    }

    private DeclaredKind KindOf(TypeDefinition type)
        => (type.Attributes & TypeAttributes.Interface) != 0 ? DeclaredKind.Interface
        : BaseTypeOf(type) switch
        {
            "System.Enum" => DeclaredKind.Enum,
            "System.MulticastDelegate" => DeclaredKind.Delegate,
            "System.ValueType" => Declares(type, "PrintMembers") ? DeclaredKind.RecordStruct : DeclaredKind.Struct,
            _ => Declares(type, "<Clone>$") ? DeclaredKind.Record : DeclaredKind.Class,
        };

    private string BaseTypeOf(TypeDefinition type)
    {
        if (type.BaseType.Kind != HandleKind.TypeReference)
            return string.Empty;

        var reference = metadata.GetTypeReference((TypeReferenceHandle)type.BaseType);
        return $"{metadata.GetString(reference.Namespace)}.{metadata.GetString(reference.Name)}";
    }

    // A record class has a clone method only the compiler can name. A record struct has no such member, so
    // it is told by the PrintMembers every record struct is given and a plain struct seldom declares.
    private bool Declares(TypeDefinition type, string method)
        => type.GetMethods().Any(handle => metadata.StringComparer.Equals(metadata.GetMethodDefinition(handle).Name, method));

    [GeneratedRegex(@"^<[^>]*>F[0-9A-F]+__(.+)$")]
    private static partial Regex FileLocal();
}

internal sealed record TypeDeclaration(
    string Name, IReadOnlyList<string> TypeParameters, string Namespace, DeclaredKind Kind, bool IsPublic,
    IReadOnlyCollection<string> Documents);
